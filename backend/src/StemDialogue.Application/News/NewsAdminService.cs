using Microsoft.EntityFrameworkCore;
using StemDialogue.Application.Abstractions;
using StemDialogue.Application.Common;
using StemDialogue.Application.Projects;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.News;

namespace StemDialogue.Application.News;

/// <summary>Create, edit and publish news without developer involvement (spec §5, §15.14).</summary>
public sealed class NewsAdminService(
    IAppDbContext db,
    ProjectResolver projects,
    ContentReferences references,
    SlugManager slugs,
    IHtmlContentSanitizer sanitizer,
    IFileStorage storage,
    TimeProvider clock)
{
    public async Task<PagedResult<NewsAdminListItem>> ListAsync(string projectSlug, NewsAdminQuery query, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var now = clock.GetUtcNow().UtcDateTime;
        var items = db.News.AsNoTracking().Where(n => n.ProjectId == project.Id);

        if (query.Status is { } status) items = items.Where(n => n.Status == status);
        if (query.Category is { } category) items = items.Where(n => n.Category == category);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            items = items.Where(n => n.Title.Ka.Contains(term) || (n.Title.En != null && n.Title.En.Contains(term)) || n.Slug.Contains(term));
        }

        var page = await items
            .OrderByDescending(n => n.PublishAtUtc ?? n.UpdatedAtUtc)
            .ToPagedAsync(query, ct);

        return page.Map(n => new NewsAdminListItem(
            n.Id,
            n.Slug,
            n.Title.Ka,
            n.Category,
            n.Status,
            n.IsLive(now),
            n.Status == PublicationStatus.Published && n.PublishAtUtc > now,
            n.PublishAtUtc,
            n.IsFeatured,
            n.UpdatedAtUtc,
            n.UpdatedBy,
            PublicPaths.News(project.Slug, n.Slug)));
    }

    public async Task<NewsAdminDto> GetAsync(string projectSlug, Guid id, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        return ToDto(await LoadAsync(project.Id, id, tracking: false, ct), project);
    }

    public async Task<NewsAdminDto> CreateAsync(string projectSlug, NewsUpsertRequest request, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var article = new NewsArticle { ProjectId = project.Id };
        await ApplyAsync(article, project, request, ct);
        db.News.Add(article);
        await db.SaveChangesAsync(ct);
        return await GetAsync(projectSlug, article.Id, ct);
    }

    public async Task<NewsAdminDto> UpdateAsync(string projectSlug, Guid id, NewsUpsertRequest request, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var article = await LoadAsync(project.Id, id, tracking: true, ct);
        if (request.ConcurrencyStamp is not { } stamp)
            throw ContentReferences.Invalid(nameof(request.ConcurrencyStamp), "The concurrency stamp is required when updating.");
        db.EnsureStamp(article, stamp);

        await ApplyAsync(article, project, request, ct);
        await db.SaveChangesAsync(ct);
        return await GetAsync(projectSlug, id, ct);
    }

    public async Task<NewsAdminDto> PublishAsync(string projectSlug, Guid id, PublishRequest request, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var article = await LoadAsync(project.Id, id, tracking: true, ct);

        if (string.IsNullOrWhiteSpace(article.Summary.Ka))
            throw ContentReferences.Invalid(nameof(NewsArticle.Summary), "Add a Georgian summary before publishing.");
        if (string.IsNullOrWhiteSpace(article.Body.Ka))
            throw ContentReferences.Invalid(nameof(NewsArticle.Body), "Add the Georgian text before publishing.");

        var now = clock.GetUtcNow().UtcDateTime;
        article.Status = PublicationStatus.Published;
        article.PublishAtUtc = request.PublishAtUtc is { } at ? DateTime.SpecifyKind(at, DateTimeKind.Utc) : now;
        article.FirstPublishedAtUtc ??= article.PublishAtUtc;

        await db.SaveChangesAsync(ct);
        return ToDto(await LoadAsync(project.Id, id, tracking: false, ct), project);
    }

    /// <summary>Returns the article to draft. Its URL keeps working once it is published again.</summary>
    public Task<NewsAdminDto> UnpublishAsync(string projectSlug, Guid id, CancellationToken ct) =>
        SetStatusAsync(projectSlug, id, PublicationStatus.Draft, ct);

    public Task<NewsAdminDto> ArchiveAsync(string projectSlug, Guid id, CancellationToken ct) =>
        SetStatusAsync(projectSlug, id, PublicationStatus.Archived, ct);

    /// <summary>Only never-published drafts can be deleted; published items are archived so links stay stable.</summary>
    public async Task DeleteAsync(string projectSlug, Guid id, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var article = await LoadAsync(project.Id, id, tracking: true, ct);
        if (article.FirstPublishedAtUtc is not null)
            throw new ConflictException("Published articles cannot be deleted. Archive the article instead.");

        db.News.Remove(article);
        await db.SaveChangesAsync(ct);
    }

    private async Task<NewsAdminDto> SetStatusAsync(string projectSlug, Guid id, PublicationStatus status, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var article = await LoadAsync(project.Id, id, tracking: true, ct);
        article.Status = status;
        if (status != PublicationStatus.Published) article.IsFeatured = false;
        await db.SaveChangesAsync(ct);
        return ToDto(await LoadAsync(project.Id, id, tracking: false, ct), project);
    }

    private async Task ApplyAsync(NewsArticle article, ProjectRef project, NewsUpsertRequest request, CancellationToken ct)
    {
        await references.EnsureImageAsync(request.CoverImageId, nameof(request.CoverImageId), ct);
        await references.EnsureImageAsync(request.ShareImageId, nameof(request.ShareImageId), ct);
        var gallery = await references.GalleryAsync(request.GalleryImageIds, nameof(request.GalleryImageIds), ct);
        var tags = await references.TagsAsync(request.TagIds, ct);

        if (request.RelatedEventId is { } eventId && !await db.Events.AnyAsync(e => e.Id == eventId && e.ProjectId == project.Id, ct))
            throw ContentReferences.Invalid(nameof(request.RelatedEventId), "The selected event does not exist in this project.");

        article.Slug = await slugs.AssignAsync(
            SluggedContentType.News,
            project.Id,
            article.Id,
            article.Slug,
            request.Slug,
            request.Title.Ka ?? string.Empty,
            article.FirstPublishedAtUtc is not null,
            candidate => db.News.AnyAsync(n => n.ProjectId == project.Id && n.Slug == candidate && n.Id != article.Id, ct),
            ct);

        article.Title.Apply(request.Title);
        article.Summary.Apply(request.Summary);
        article.Body.ApplyHtml(request.Body, sanitizer);
        article.Category = request.Category;
        article.CoverImageId = request.CoverImageId;
        article.RelatedEventId = request.RelatedEventId;
        article.IsFeatured = request.IsFeatured;
        article.SeoTitle.Apply(request.SeoTitle);
        article.SeoDescription.Apply(request.SeoDescription);
        article.ShareImageId = request.ShareImageId;
        article.Tags.SyncById(tags);
        article.Gallery.SyncOrdered(
            gallery,
            g => g.MediaAssetId,
            mediaId => new NewsGalleryItem { NewsArticleId = article.Id, MediaAssetId = mediaId },
            (g, order) => g.SortOrder = order);
    }

    private async Task<NewsArticle> LoadAsync(Guid projectId, Guid id, bool tracking, CancellationToken ct)
    {
        var query = db.News
            .Include(n => n.CoverImage)
            .Include(n => n.ShareImage)
            .Include(n => n.Tags)
            .Include(n => n.Gallery).ThenInclude(g => g.MediaAsset)
            .AsSplitQuery();

        return await (tracking ? query : query.AsNoTracking())
            .FirstOrDefaultAsync(n => n.Id == id && n.ProjectId == projectId, ct)
            ?? throw new NotFoundException("News", id);
    }

    private NewsAdminDto ToDto(NewsArticle n, ProjectRef project) => new(
        n.Id,
        n.Slug,
        n.Title.ToDto(),
        n.Summary.ToDto(),
        n.Body.ToDto(),
        n.Category,
        n.Status,
        n.PublishAtUtc,
        n.FirstPublishedAtUtc,
        n.IsFeatured,
        n.CoverImage.ToRef(storage),
        n.Gallery.OrderBy(g => g.SortOrder).Select(g => g.MediaAsset.ToRef(storage)!).ToList(),
        n.Tags.Select(t => t.Id).ToList(),
        n.RelatedEventId,
        n.SeoTitle.ToDto(),
        n.SeoDescription.ToDto(),
        n.ShareImage.ToRef(storage),
        PublicPaths.News(project.Slug, n.Slug),
        AuditInfo.Of(n));
}
