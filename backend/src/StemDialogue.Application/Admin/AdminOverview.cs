using Microsoft.EntityFrameworkCore;
using StemDialogue.Application.Abstractions;
using StemDialogue.Application.Common;
using StemDialogue.Application.Projects;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.Events;
using StemDialogue.Domain.Media;
using StemDialogue.Domain.News;
using StemDialogue.Domain.Pages;
using StemDialogue.Domain.People;
using StemDialogue.Domain.Projects;
using StemDialogue.Domain.Resources;

namespace StemDialogue.Application.Admin;

public sealed record DashboardDto(
    int PublishedNews,
    int DraftNews,
    int ScheduledNews,
    int UpcomingEvents,
    int EventsAwaitingCompletion,
    int PublishedPeople,
    int DraftPeople,
    int PublishedResources,
    int ImagesWithoutAltText,
    long MediaBytes,
    IReadOnlyList<AuditEntryDto> RecentChanges);

public sealed record AuditEntryDto(long Id, DateTime OccurredAtUtc, string? UserName, string EntityType, string EntityId, string Action, string? ChangesJson);

public sealed class AuditQuery : PageQuery
{
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? UserId { get; set; }
}

/// <summary>Dropdown values for admin forms, with Georgian/English labels.</summary>
public sealed record LookupsDto(
    IReadOnlyList<LabeledValue> NewsCategories,
    IReadOnlyList<LabeledValue> EventTypes,
    IReadOnlyList<LabeledValue> EventFormats,
    IReadOnlyList<LabeledValue> EventStatuses,
    IReadOnlyList<LabeledValue> ParticipantRoles,
    IReadOnlyList<LabeledValue> PersonRoles,
    IReadOnlyList<LabeledValue> PersonLinkTypes,
    IReadOnlyList<LabeledValue> ResourceTypes,
    IReadOnlyList<LabeledValue> PartnerRoles,
    IReadOnlyList<LabeledValue> SocialPlatforms,
    IReadOnlyList<LabeledValue> PublicationStatuses,
    IReadOnlyList<string> PageKeys,
    IReadOnlyList<string> AllowedUploadExtensions,
    IReadOnlyList<LookupItem> People,
    IReadOnlyList<LookupItem> Events,
    IReadOnlyList<LookupItem> Tags,
    IReadOnlyList<LookupItem> Disciplines,
    IReadOnlyList<LookupItem> Organizations,
    IReadOnlyList<LookupItem> Projects);

public sealed record LookupItem(Guid Id, string Label);

public sealed class AdminOverviewService(IAppDbContext db, ProjectResolver projects, TimeProvider clock)
{
    public async Task<DashboardDto> DashboardAsync(string projectSlug, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var now = clock.GetUtcNow().UtcDateTime;
        var news = db.News.AsNoTracking().Where(n => n.ProjectId == project.Id);
        var events = db.Events.AsNoTracking().Where(e => e.ProjectId == project.Id);
        var people = db.People.AsNoTracking().Where(p => p.Roles.Any(r => r.ProjectId == project.Id));

        var recent = await db.AuditEntries.AsNoTracking().OrderByDescending(a => a.Id).Take(10).ToListAsync(ct);

        return new DashboardDto(
            await news.CountAsync(n => n.Status == PublicationStatus.Published && n.PublishAtUtc <= now, ct),
            await news.CountAsync(n => n.Status == PublicationStatus.Draft, ct),
            await news.CountAsync(n => n.Status == PublicationStatus.Published && n.PublishAtUtc > now, ct),
            await events.CountAsync(e => e.Status != EventStatus.Completed && e.Status != EventStatus.Cancelled && e.StartsAtUtc >= now, ct),
            await events.CountAsync(e => (e.Status == EventStatus.Planned || e.Status == EventStatus.Postponed) && (e.EndsAtUtc ?? e.StartsAtUtc) < now, ct),
            await people.CountAsync(p => p.Status == PublicationStatus.Published, ct),
            await people.CountAsync(p => p.Status == PublicationStatus.Draft, ct),
            await db.Resources.CountAsync(r => r.ProjectId == project.Id && r.Status == PublicationStatus.Published, ct),
            await db.Media.CountAsync(m => m.Kind == MediaKind.Image && m.AltText.Ka == string.Empty, ct),
            await db.Media.SumAsync(m => (long?)m.SizeBytes, ct) ?? 0,
            recent.Select(ToDto).ToList());
    }

    public async Task<PagedResult<AuditEntryDto>> AuditAsync(AuditQuery query, CancellationToken ct)
    {
        var items = db.AuditEntries.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.EntityType)) items = items.Where(a => a.EntityType == query.EntityType);
        if (!string.IsNullOrWhiteSpace(query.EntityId)) items = items.Where(a => a.EntityId == query.EntityId);
        if (!string.IsNullOrWhiteSpace(query.UserId)) items = items.Where(a => a.UserId == query.UserId);

        var page = await items.OrderByDescending(a => a.Id).ToPagedAsync(query, ct);
        return page.Map(ToDto);
    }

    public async Task<LookupsDto> LookupsAsync(string projectSlug, string language, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);

        var people = await db.People.AsNoTracking().OrderBy(p => p.FullName.Ka)
            .Select(p => new LookupItem(p.Id, p.FullName.Ka)).ToListAsync(ct);
        var events = await db.Events.AsNoTracking().Where(e => e.ProjectId == project.Id).OrderByDescending(e => e.StartsAtUtc)
            .Select(e => new { e.Id, e.Title.Ka, e.StartsAtUtc }).ToListAsync(ct);
        var tags = await db.Tags.AsNoTracking().OrderBy(t => t.Name.Ka).Select(t => new LookupItem(t.Id, t.Name.Ka)).ToListAsync(ct);
        var disciplines = await db.Disciplines.AsNoTracking().OrderBy(d => d.DisplayOrder)
            .Select(d => new LookupItem(d.Id, d.Name.Ka)).ToListAsync(ct);
        var organizations = await db.Organizations.AsNoTracking().OrderBy(o => o.Name.Ka)
            .Select(o => new LookupItem(o.Id, o.Name.Ka)).ToListAsync(ct);
        var allProjects = await db.Projects.AsNoTracking().OrderBy(p => p.StartDate)
            .Select(p => new LookupItem(p.Id, p.Name.Ka)).ToListAsync(ct);

        return new LookupsDto(
            EnumLabels.All<NewsCategory>(language),
            EnumLabels.All<EventType>(language),
            EnumLabels.All<EventFormat>(language),
            EnumLabels.All<EventStatus>(language),
            EnumLabels.All<EventParticipantRole>(language),
            EnumLabels.All<PersonRole>(language),
            EnumLabels.All<PersonLinkType>(language),
            EnumLabels.All<ResourceType>(language),
            EnumLabels.All<PartnerRole>(language),
            EnumLabels.All<SocialPlatform>(language),
            EnumLabels.All<PublicationStatus>(language),
            PageKeys.All,
            Media.MediaInspector.AllowedExtensions.Order().ToList(),
            people,
            events.Select(e => new LookupItem(e.Id, $"{TbilisiTime.ToLocal(e.StartsAtUtc):yyyy-MM-dd} · {e.Ka}")).ToList(),
            tags,
            disciplines,
            organizations,
            allProjects);
    }

    private static AuditEntryDto ToDto(AuditEntry a) => new(a.Id, a.OccurredAtUtc, a.UserName, a.EntityType, a.EntityId, a.Action, a.ChangesJson);
}
