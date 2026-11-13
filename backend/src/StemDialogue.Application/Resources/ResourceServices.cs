using Microsoft.EntityFrameworkCore;
using StemDialogue.Application.Abstractions;
using StemDialogue.Application.Common;
using StemDialogue.Application.Projects;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.Resources;

namespace StemDialogue.Application.Resources;

/// <summary>Learning materials: videos, presentations, documents and links (spec §9).</summary>
public sealed class PublicResourceService(
    IAppDbContext db,
    ProjectResolver projects,
    ContentCards cards,
    PublicMapper map,
    SlugManager slugs)
{
    public async Task<PagedResult<ResourceCardDto>> ListAsync(string projectSlug, ResourceQuery query, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var items = db.Resources.AsNoTracking().Live(project.Id, cards.UtcNow);

        if (query.Type is { } type) items = items.Where(r => r.Type == type);
        if (!string.IsNullOrWhiteSpace(query.Event)) items = items.Where(r => r.Event != null && r.Event.Slug == query.Event);
        if (!string.IsNullOrWhiteSpace(query.Discipline)) items = items.Where(r => r.Disciplines.Any(d => d.Slug == query.Discipline));
        if (!string.IsNullOrWhiteSpace(query.Tag)) items = items.Where(r => r.Tags.Any(t => t.Slug == query.Tag));
        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var term = query.Q.Trim();
            items = items.Where(r =>
                r.Title.Ka.Contains(term) || (r.Title.En != null && r.Title.En.Contains(term)) ||
                r.Description.Ka.Contains(term) || r.AuthorName.Ka.Contains(term));
        }

        var page = await items
            .OrderByDescending(r => r.IsFeatured)
            .ThenByDescending(r => r.PublishedAtUtc ?? r.CreatedAtUtc)
            .WithCardData()
            .ToPagedAsync(query, ct);

        return page.Map(r => cards.Resource(r, project));
    }

    public async Task<ResourceDetailDto> GetAsync(string projectSlug, string slug, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var now = cards.UtcNow;

        var resource = await db.Resources.AsNoTracking()
            .Live(project.Id, now)
            .WithCardData()
            .Include(r => r.Disciplines)
            .FirstOrDefaultAsync(r => r.Slug == slug, ct);

        if (resource is null)
        {
            var targetId = await slugs.FindRedirectAsync(SluggedContentType.Resource, project.Id, slug, ct);
            var moved = targetId == Guid.Empty
                ? null
                : await db.Resources.AsNoTracking().Live(project.Id, now).Where(r => r.Id == targetId).Select(r => r.Slug).FirstOrDefaultAsync(ct);
            throw moved is null ? new NotFoundException("Resource", slug) : (Exception)new SlugMovedException(moved);
        }

        var related = await db.Resources.AsNoTracking()
            .Live(project.Id, now)
            .Where(r => r.Id != resource.Id && (resource.EventId != null ? r.EventId == resource.EventId : r.Type == resource.Type))
            .OrderByDescending(r => r.PublishedAtUtc ?? r.CreatedAtUtc)
            .Take(4)
            .WithCardData()
            .ToListAsync(ct);

        var card = cards.Resource(resource, project);
        return new ResourceDetailDto(
            card,
            map.Terms(resource.Disciplines),
            related.Select(r => cards.Resource(r, project)).ToList(),
            map.Seo(null, null, card.Title, card.Description, resource.Thumbnail ?? (resource.File?.Kind == Domain.Media.MediaKind.Image ? resource.File : null), card.Path));
    }
}

public sealed class ResourceAdminService(
    IAppDbContext db,
    ProjectResolver projects,
    ContentReferences references,
    SlugManager slugs,
    IFileStorage storage,
    TimeProvider clock)
{
    public async Task<PagedResult<ResourceAdminListItem>> ListAsync(string projectSlug, ResourceAdminQuery query, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var items = db.Resources.AsNoTracking().Where(r => r.ProjectId == project.Id);

        if (query.Type is { } type) items = items.Where(r => r.Type == type);
        if (query.Status is { } status) items = items.Where(r => r.Status == status);
        if (query.EventId is { } eventId) items = items.Where(r => r.EventId == eventId);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            items = items.Where(r => r.Title.Ka.Contains(term) || (r.Title.En != null && r.Title.En.Contains(term)) || r.Slug.Contains(term));
        }

        var page = await items
            .OrderByDescending(r => r.UpdatedAtUtc)
            .Include(r => r.Event)
            .ToPagedAsync(query, ct);

        return page.Map(r => new ResourceAdminListItem(
            r.Id, r.Slug, r.Title.Ka, r.Type, r.Status, r.IsFeatured, r.Event?.Title.Ka, r.PublishedAtUtc, r.UpdatedAtUtc, r.UpdatedBy));
    }

    public async Task<ResourceAdminDto> GetAsync(string projectSlug, Guid id, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        return ToDto(await LoadAsync(project.Id, id, tracking: false, ct), project);
    }

    public async Task<ResourceAdminDto> CreateAsync(string projectSlug, ResourceUpsertRequest request, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var resource = new Resource { ProjectId = project.Id };
        await ApplyAsync(resource, project, request, ct);
        db.Resources.Add(resource);
        await db.SaveChangesAsync(ct);
        return await GetAsync(projectSlug, resource.Id, ct);
    }

    public async Task<ResourceAdminDto> UpdateAsync(string projectSlug, Guid id, ResourceUpsertRequest request, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var resource = await LoadAsync(project.Id, id, tracking: true, ct);
        if (request.ConcurrencyStamp is not { } stamp)
            throw ContentReferences.Invalid(nameof(request.ConcurrencyStamp), "The concurrency stamp is required when updating.");
        db.EnsureStamp(resource, stamp);

        await ApplyAsync(resource, project, request, ct);
        await db.SaveChangesAsync(ct);
        return await GetAsync(projectSlug, id, ct);
    }

    public async Task DeleteAsync(string projectSlug, Guid id, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var resource = await LoadAsync(project.Id, id, tracking: true, ct);
        if (resource.Status == PublicationStatus.Published)
            throw new ConflictException("Archive the resource before deleting it.");
        db.Resources.Remove(resource);
        await db.SaveChangesAsync(ct);
    }

    private async Task ApplyAsync(Resource resource, ProjectRef project, ResourceUpsertRequest request, CancellationToken ct)
    {
        await references.EnsureFileAsync(request.FileId, nameof(request.FileId), ct);
        await references.EnsureImageAsync(request.ThumbnailId, nameof(request.ThumbnailId), ct);
        var tags = await references.TagsAsync(request.TagIds, ct);
        var disciplines = await references.DisciplinesAsync(request.DisciplineIds, ct);

        if (request.EventId is { } eventId && !await db.Events.AnyAsync(e => e.Id == eventId && e.ProjectId == project.Id, ct))
            throw ContentReferences.Invalid(nameof(request.EventId), "The selected event does not exist in this project.");
        if (request.AuthorId is { } authorId && !await db.People.AnyAsync(p => p.Id == authorId, ct))
            throw ContentReferences.Invalid(nameof(request.AuthorId), "The selected person does not exist.");

        var wasPublic = resource.Status == PublicationStatus.Published;
        resource.Slug = await slugs.AssignAsync(
            SluggedContentType.Resource,
            project.Id,
            resource.Id,
            resource.Slug,
            request.Slug,
            request.Title.Ka ?? string.Empty,
            wasPublic,
            candidate => db.Resources.AnyAsync(r => r.ProjectId == project.Id && r.Slug == candidate && r.Id != resource.Id, ct),
            ct);

        resource.Title.Apply(request.Title);
        resource.Description.Apply(request.Description);
        resource.Type = request.Type;
        resource.FileId = request.FileId;
        resource.ExternalUrl = TextUtils.NullIfBlank(request.ExternalUrl);
        resource.AuthorId = request.AuthorId;
        resource.AuthorName.Apply(request.AuthorName);
        resource.EventId = request.EventId;
        resource.ThumbnailId = request.ThumbnailId;
        resource.Status = request.Status;
        resource.IsFeatured = request.IsFeatured;
        resource.PublishedAtUtc = request.PublishedAtUtc is { } at
            ? DateTime.SpecifyKind(at, DateTimeKind.Utc)
            : resource.PublishedAtUtc ?? (request.Status == PublicationStatus.Published ? clock.GetUtcNow().UtcDateTime : null);
        resource.Tags.SyncById(tags);
        resource.Disciplines.SyncById(disciplines);
    }

    private async Task<Resource> LoadAsync(Guid projectId, Guid id, bool tracking, CancellationToken ct)
    {
        var query = db.Resources
            .Include(r => r.File)
            .Include(r => r.Thumbnail)
            .Include(r => r.Tags)
            .Include(r => r.Disciplines)
            .AsSplitQuery();

        return await (tracking ? query : query.AsNoTracking()).FirstOrDefaultAsync(r => r.Id == id && r.ProjectId == projectId, ct)
            ?? throw new NotFoundException("Resource", id);
    }

    private ResourceAdminDto ToDto(Resource r, ProjectRef project) => new(
        r.Id,
        r.Slug,
        r.Title.ToDto(),
        r.Description.ToDto(),
        r.Type,
        r.File.ToRef(storage),
        r.ExternalUrl,
        r.AuthorId,
        r.AuthorName.ToDto(),
        r.EventId,
        r.Thumbnail.ToRef(storage),
        r.Status,
        r.PublishedAtUtc,
        r.IsFeatured,
        r.Tags.Select(t => t.Id).ToList(),
        r.Disciplines.Select(d => d.Id).ToList(),
        PublicPaths.Resource(project.Slug, r.Slug),
        AuditInfo.Of(r));
}
