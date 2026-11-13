using Microsoft.EntityFrameworkCore;
using StemDialogue.Application.Abstractions;
using StemDialogue.Application.Common;
using StemDialogue.Application.Projects;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.Events;

namespace StemDialogue.Application.Events;

/// <summary>
/// Event administration. Lifecycle changes go through dedicated operations so the public URL never changes
/// between the announcement and the archive (spec §6, §15.15).
/// </summary>
public sealed class EventAdminService(
    IAppDbContext db,
    ProjectResolver projects,
    ContentReferences references,
    SlugManager slugs,
    IHtmlContentSanitizer sanitizer,
    IFileStorage storage,
    TimeProvider clock)
{
    public async Task<PagedResult<EventAdminListItem>> ListAsync(string projectSlug, EventAdminQuery query, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var now = clock.GetUtcNow().UtcDateTime;
        var items = db.Events.AsNoTracking().Where(e => e.ProjectId == project.Id);

        items = query.Scope switch
        {
            EventScope.Upcoming => items.Where(e => (e.EndsAtUtc ?? e.StartsAtUtc) >= now),
            EventScope.Past => items.Where(e => (e.EndsAtUtc ?? e.StartsAtUtc) < now),
            _ => items,
        };
        if (query.Status is { } status) items = items.Where(e => e.Status == status);
        if (query.Publication is { } publication) items = items.Where(e => e.Publication == publication);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            items = items.Where(e => e.Title.Ka.Contains(term) || (e.Title.En != null && e.Title.En.Contains(term)) || e.Slug.Contains(term));
        }

        var page = await items
            .OrderByDescending(e => e.StartsAtUtc)
            .Include(e => e.Participants)
            .ToPagedAsync(query, ct);

        return page.Map(e => new EventAdminListItem(
            e.Id,
            e.Slug,
            e.Title.Ka,
            e.Type,
            e.Status,
            e.Publication,
            e.StartsAtUtc,
            e.IsFeatured,
            e.Status is EventStatus.Planned or EventStatus.Postponed && (e.EndsAtUtc ?? e.StartsAtUtc) < now,
            e.Participants.Count,
            e.UpdatedAtUtc,
            e.UpdatedBy,
            PublicPaths.Event(project.Slug, e.Slug)));
    }

    public async Task<EventAdminDto> GetAsync(string projectSlug, Guid id, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        return ToDto(await LoadAsync(project.Id, id, tracking: false, ct), project);
    }

    public async Task<EventAdminDto> CreateAsync(string projectSlug, EventUpsertRequest request, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var item = new Event { ProjectId = project.Id };
        await ApplyAsync(item, project, request, ct);
        db.Events.Add(item);
        await db.SaveChangesAsync(ct);
        return await GetAsync(projectSlug, item.Id, ct);
    }

    public async Task<EventAdminDto> UpdateAsync(string projectSlug, Guid id, EventUpsertRequest request, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var item = await LoadAsync(project.Id, id, tracking: true, ct);
        if (request.ConcurrencyStamp is not { } stamp)
            throw ContentReferences.Invalid(nameof(request.ConcurrencyStamp), "The concurrency stamp is required when updating.");
        db.EnsureStamp(item, stamp);

        await ApplyAsync(item, project, request, ct);
        await db.SaveChangesAsync(ct);
        return await GetAsync(projectSlug, id, ct);
    }

    public Task<EventAdminDto> PublishAsync(string projectSlug, Guid id, CancellationToken ct) =>
        MutateAsync(projectSlug, id, item =>
        {
            if (string.IsNullOrWhiteSpace(item.Summary.Ka) && string.IsNullOrWhiteSpace(item.Description.Ka))
                throw ContentReferences.Invalid(nameof(Event.Summary), "Add a Georgian summary or description before publishing.");
            item.Publication = PublicationStatus.Published;
            item.FirstPublishedAtUtc ??= clock.GetUtcNow().UtcDateTime;
        }, ct);

    public Task<EventAdminDto> UnpublishAsync(string projectSlug, Guid id, CancellationToken ct) =>
        MutateAsync(projectSlug, id, item =>
        {
            item.Publication = PublicationStatus.Draft;
            item.IsFeatured = false;
        }, ct);

    /// <summary>Marks the event as held. It moves to the archive at the same URL.</summary>
    public Task<EventAdminDto> CompleteAsync(string projectSlug, Guid id, CompleteEventRequest request, CancellationToken ct) =>
        MutateAsync(projectSlug, id, item =>
        {
            item.Complete(clock.GetUtcNow().UtcDateTime);
            if (request.Recap is not null && !string.IsNullOrWhiteSpace(request.Recap.Ka)) item.Recap.ApplyHtml(request.Recap, sanitizer);
            if (!string.IsNullOrWhiteSpace(request.RecordingUrl)) item.RecordingUrl = request.RecordingUrl.Trim();
        }, ct);

    public Task<EventAdminDto> CancelAsync(string projectSlug, Guid id, CancelEventRequest request, CancellationToken ct) =>
        MutateAsync(projectSlug, id, item => item.Cancel(request.Note.Ka ?? string.Empty, request.Note.En), ct);

    public Task<EventAdminDto> PostponeAsync(string projectSlug, Guid id, PostponeEventRequest request, CancellationToken ct) =>
        MutateAsync(projectSlug, id, item => item.Postpone(
            DateTime.SpecifyKind(request.NewStartsAtUtc, DateTimeKind.Utc),
            request.NewEndsAtUtc is null ? null : DateTime.SpecifyKind(request.NewEndsAtUtc.Value, DateTimeKind.Utc),
            request.Note.Ka ?? string.Empty,
            request.Note.En,
            clock.GetUtcNow().UtcDateTime), ct);

    public Task<EventAdminDto> RestoreAsync(string projectSlug, Guid id, CancellationToken ct) =>
        MutateAsync(projectSlug, id, item => item.Restore(), ct);

    /// <summary>Only never-published events can be deleted; cancel or unpublish public ones instead.</summary>
    public async Task DeleteAsync(string projectSlug, Guid id, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var item = await LoadAsync(project.Id, id, tracking: true, ct);
        if (item.FirstPublishedAtUtc is not null)
            throw new ConflictException("Published events cannot be deleted. Cancel or unpublish the event instead.");
        if (await db.Resources.AnyAsync(r => r.EventId == id, ct) || await db.News.AnyAsync(n => n.RelatedEventId == id, ct))
            throw new ConflictException("Resources or news are linked to this event. Unlink them first.");

        db.Events.Remove(item);
        await db.SaveChangesAsync(ct);
    }

    private async Task<EventAdminDto> MutateAsync(string projectSlug, Guid id, Action<Event> change, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var item = await LoadAsync(project.Id, id, tracking: true, ct);
        change(item);
        await db.SaveChangesAsync(ct);
        return ToDto(await LoadAsync(project.Id, id, tracking: false, ct), project);
    }

    private async Task ApplyAsync(Event item, ProjectRef project, EventUpsertRequest request, CancellationToken ct)
    {
        await references.EnsureImageAsync(request.CoverImageId, nameof(request.CoverImageId), ct);
        await references.EnsureImageAsync(request.ShareImageId, nameof(request.ShareImageId), ct);
        var gallery = await references.GalleryAsync(request.GalleryImageIds, nameof(request.GalleryImageIds), ct);
        var tags = await references.TagsAsync(request.TagIds, ct);
        var disciplines = await references.DisciplinesAsync(request.DisciplineIds, ct);

        var participants = request.Participants ?? [];
        var personIds = participants.Select(p => p.PersonId).ToList();
        if (personIds.Count > 0 && await db.People.CountAsync(p => personIds.Contains(p.Id), ct) != personIds.Count)
            throw ContentReferences.Invalid(nameof(request.Participants), "One or more people do not exist.");

        item.Slug = await slugs.AssignAsync(
            SluggedContentType.Event,
            project.Id,
            item.Id,
            item.Slug,
            request.Slug,
            request.Title.Ka ?? string.Empty,
            item.FirstPublishedAtUtc is not null,
            candidate => db.Events.AnyAsync(e => e.ProjectId == project.Id && e.Slug == candidate && e.Id != item.Id, ct),
            ct);

        item.Type = request.Type;
        item.Title.Apply(request.Title);
        item.Summary.Apply(request.Summary);
        item.Description.ApplyHtml(request.Description, sanitizer);
        item.StartsAtUtc = DateTime.SpecifyKind(request.StartsAtUtc, DateTimeKind.Utc);
        item.EndsAtUtc = request.EndsAtUtc is null ? null : DateTime.SpecifyKind(request.EndsAtUtc.Value, DateTimeKind.Utc);
        item.Format = request.Format;
        item.Venue.Apply(request.Venue);
        item.StreamUrl = TextUtils.NullIfBlank(request.StreamUrl);
        item.RegistrationUrl = TextUtils.NullIfBlank(request.RegistrationUrl);
        item.RegistrationDeadlineUtc = request.RegistrationDeadlineUtc is null ? null : DateTime.SpecifyKind(request.RegistrationDeadlineUtc.Value, DateTimeKind.Utc);
        item.CoverImageId = request.CoverImageId;
        item.IsFeatured = request.IsFeatured;
        item.Recap.ApplyHtml(request.Recap, sanitizer);
        item.RecordingUrl = TextUtils.NullIfBlank(request.RecordingUrl);
        item.SeoTitle.Apply(request.SeoTitle);
        item.SeoDescription.Apply(request.SeoDescription);
        item.ShareImageId = request.ShareImageId;
        item.Tags.SyncById(tags);
        item.Disciplines.SyncById(disciplines);
        item.Gallery.SyncOrdered(
            gallery,
            g => g.MediaAssetId,
            mediaId => new EventGalleryItem { EventId = item.Id, MediaAssetId = mediaId },
            (g, order) => g.SortOrder = order);
        item.Participants.SyncOrdered(
            personIds,
            p => p.PersonId,
            personId => new EventParticipant { EventId = item.Id, PersonId = personId },
            (p, order) => p.SortOrder = order);
        foreach (var participant in item.Participants)
            participant.Role = participants.First(p => p.PersonId == participant.PersonId).Role;
    }

    private async Task<Event> LoadAsync(Guid projectId, Guid id, bool tracking, CancellationToken ct)
    {
        var query = db.Events
            .Include(e => e.CoverImage)
            .Include(e => e.ShareImage)
            .Include(e => e.Tags)
            .Include(e => e.Disciplines)
            .Include(e => e.Gallery).ThenInclude(g => g.MediaAsset)
            .Include(e => e.Participants).ThenInclude(p => p.Person)
            .AsSplitQuery();

        return await (tracking ? query : query.AsNoTracking())
            .FirstOrDefaultAsync(e => e.Id == id && e.ProjectId == projectId, ct)
            ?? throw new NotFoundException("Event", id);
    }

    private EventAdminDto ToDto(Event e, ProjectRef project) => new(
        e.Id,
        e.Slug,
        e.Type,
        e.Title.ToDto(),
        e.Summary.ToDto(),
        e.Description.ToDto(),
        e.StartsAtUtc,
        e.EndsAtUtc,
        e.OriginalStartsAtUtc,
        e.Format,
        e.Venue.ToDto(),
        e.StreamUrl,
        e.RegistrationUrl,
        e.RegistrationDeadlineUtc,
        e.Status,
        e.StatusNote.ToDto(),
        e.Publication,
        e.FirstPublishedAtUtc,
        e.IsFeatured,
        e.CoverImage.ToRef(storage),
        e.Participants
            .OrderBy(p => p.SortOrder)
            .Select(p => new EventParticipantAdminDto(p.PersonId, p.Person.FullName.Ka, p.Role, p.SortOrder))
            .ToList(),
        e.Tags.Select(t => t.Id).ToList(),
        e.Disciplines.Select(d => d.Id).ToList(),
        e.Recap.ToDto(),
        e.RecordingUrl,
        e.CompletedAtUtc,
        e.Gallery.OrderBy(g => g.SortOrder).Select(g => g.MediaAsset.ToRef(storage)!).ToList(),
        e.SeoTitle.ToDto(),
        e.SeoDescription.ToDto(),
        e.ShareImage.ToRef(storage),
        PublicPaths.Event(project.Slug, e.Slug),
        AuditInfo.Of(e));
}
