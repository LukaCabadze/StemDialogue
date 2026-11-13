using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StemDialogue.Application.Abstractions;
using StemDialogue.Application.Common;
using StemDialogue.Application.Pages;
using StemDialogue.Application.Projects;
using StemDialogue.Domain.Ambassadors;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.Pages;

namespace StemDialogue.Application.Ambassadors;

// ---------- Models ----------

public sealed record SchoolActivityCardDto(
    string Slug,
    string Title,
    string? Summary,
    string? SchoolName,
    string? Region,
    DateOnly? ActivityDate,
    MediaDto? Cover,
    string Path);

public sealed record SchoolActivityDetailDto(
    SchoolActivityCardDto Activity,
    string? BodyHtml,
    IReadOnlyList<MediaDto> Gallery,
    SeoDto Seo);

/// <summary>Program information page plus the school-activity archive (spec §8).</summary>
public sealed record AmbassadorsDto(PageDto? Page, IReadOnlyList<SchoolActivityCardDto> LatestActivities, int ActivityCount);

public sealed class SchoolActivityAdminQuery : PageQuery
{
    public PublicationStatus? Status { get; set; }
    public string? Search { get; set; }
}

public sealed record SchoolActivityUpsertRequest(
    LocalizedTextDto Title,
    string? Slug,
    LocalizedTextDto? Summary,
    LocalizedTextDto? Body,
    LocalizedTextDto? SchoolName,
    LocalizedTextDto? Region,
    DateOnly? ActivityDate,
    Guid? CoverImageId,
    IReadOnlyList<Guid>? GalleryImageIds,
    PublicationStatus Status,
    bool MediaConsentConfirmed,
    string? ConsentReference,
    Guid? ConcurrencyStamp);

public sealed record SchoolActivityAdminListItem(Guid Id, string Slug, string Title, string? SchoolName, DateOnly? ActivityDate, PublicationStatus Status, bool MediaConsentConfirmed, DateTime UpdatedAtUtc);

public sealed record SchoolActivityAdminDto(
    Guid Id,
    string Slug,
    LocalizedTextDto Title,
    LocalizedTextDto Summary,
    LocalizedTextDto Body,
    LocalizedTextDto SchoolName,
    LocalizedTextDto Region,
    DateOnly? ActivityDate,
    AdminMediaRef? CoverImage,
    IReadOnlyList<AdminMediaRef> Gallery,
    PublicationStatus Status,
    bool MediaConsentConfirmed,
    string? ConsentReference,
    string PublicPath,
    AuditInfo Audit);

public sealed class SchoolActivityUpsertValidator : AbstractValidator<SchoolActivityUpsertRequest>
{
    public SchoolActivityUpsertValidator()
    {
        RuleFor(x => x.Title).RequiredText(Limits.Title);
        RuleFor(x => x.Slug).OptionalSlug();
        RuleFor(x => x.Summary).OptionalText(Limits.Summary);
        RuleFor(x => x.Body).OptionalText(Limits.Html);
        RuleFor(x => x.SchoolName).OptionalText(Limits.Short);
        RuleFor(x => x.Region).OptionalText(Limits.Name);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.ConsentReference).MaximumLength(300);

        // Minors: no identifiable imagery is published without a recorded legal basis (spec §8).
        RuleFor(x => x.MediaConsentConfirmed)
            .Equal(true)
            .When(x => x.Status == PublicationStatus.Published && (x.CoverImageId is not null || x.GalleryImageIds is { Count: > 0 }))
            .WithMessage("Confirm that consent and permissions exist before publishing photos from school activities.");
    }
}

// ---------- Services ----------

public sealed class PublicAmbassadorService(
    IAppDbContext db,
    ProjectResolver projects,
    PublicPageService pages,
    PublicMapper map,
    SlugManager slugs)
{
    public async Task<AmbassadorsDto> OverviewAsync(string projectSlug, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var page = await pages.FindAsync(project, PageKeys.StemAmbassadors, ct);
        var query = Live(project.Id);
        var latest = await query.OrderByDescending(a => a.ActivityDate).ThenByDescending(a => a.CreatedAtUtc).Take(6).Include(a => a.CoverImage).ToListAsync(ct);
        return new AmbassadorsDto(page, latest.Select(a => Card(a, project)).ToList(), await query.CountAsync(ct));
    }

    public async Task<PagedResult<SchoolActivityCardDto>> ListAsync(string projectSlug, PageQuery paging, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var page = await Live(project.Id)
            .OrderByDescending(a => a.ActivityDate).ThenByDescending(a => a.CreatedAtUtc)
            .Include(a => a.CoverImage)
            .ToPagedAsync(paging, ct);
        return page.Map(a => Card(a, project));
    }

    public async Task<SchoolActivityDetailDto> GetAsync(string projectSlug, string slug, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var activity = await Live(project.Id)
            .Include(a => a.CoverImage)
            .Include(a => a.Gallery).ThenInclude(g => g.MediaAsset)
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.Slug == slug, ct);

        if (activity is null)
        {
            var targetId = await slugs.FindRedirectAsync(SluggedContentType.SchoolActivity, project.Id, slug, ct);
            var moved = targetId == Guid.Empty ? null : await Live(project.Id).Where(a => a.Id == targetId).Select(a => a.Slug).FirstOrDefaultAsync(ct);
            throw moved is null ? new NotFoundException("School activity", slug) : (Exception)new SlugMovedException(moved);
        }

        var card = Card(activity, project);
        return new SchoolActivityDetailDto(
            card,
            map.TextOrNull(activity.Body),
            activity.MediaConsentConfirmed
                ? activity.Gallery.OrderBy(g => g.SortOrder).Select(g => map.Media(g.MediaAsset)!).ToList()
                : [],
            map.Seo(null, null, card.Title, card.Summary ?? map.Text(activity.Body), activity.MediaConsentConfirmed ? activity.CoverImage : null, card.Path));
    }

    private IQueryable<SchoolActivity> Live(Guid projectId) =>
        db.SchoolActivities.AsNoTracking().Where(a => a.ProjectId == projectId && a.Status == PublicationStatus.Published);

    private SchoolActivityCardDto Card(SchoolActivity a, ProjectRef project) => new(
        a.Slug,
        map.Text(a.Title),
        map.TextOrNull(a.Summary),
        map.TextOrNull(a.SchoolName),
        map.TextOrNull(a.Region),
        a.ActivityDate,
        a.MediaConsentConfirmed ? map.Media(a.CoverImage) : null,
        PublicPaths.SchoolActivity(project.Slug, a.Slug));
}

public sealed class SchoolActivityAdminService(
    IAppDbContext db,
    ProjectResolver projects,
    ContentReferences references,
    SlugManager slugs,
    IHtmlContentSanitizer sanitizer,
    IFileStorage storage)
{
    public async Task<PagedResult<SchoolActivityAdminListItem>> ListAsync(string projectSlug, SchoolActivityAdminQuery query, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var items = db.SchoolActivities.AsNoTracking().Where(a => a.ProjectId == project.Id);
        if (query.Status is { } status) items = items.Where(a => a.Status == status);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            items = items.Where(a => a.Title.Ka.Contains(term) || a.SchoolName.Ka.Contains(term));
        }

        var page = await items.OrderByDescending(a => a.ActivityDate).ThenByDescending(a => a.UpdatedAtUtc).ToPagedAsync(query, ct);
        return page.Map(a => new SchoolActivityAdminListItem(a.Id, a.Slug, a.Title.Ka, TextUtils.NullIfBlank(a.SchoolName.Ka), a.ActivityDate, a.Status, a.MediaConsentConfirmed, a.UpdatedAtUtc));
    }

    public async Task<SchoolActivityAdminDto> GetAsync(string projectSlug, Guid id, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        return ToDto(await LoadAsync(project.Id, id, tracking: false, ct), project);
    }

    public async Task<SchoolActivityAdminDto> CreateAsync(string projectSlug, SchoolActivityUpsertRequest request, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var activity = new SchoolActivity { ProjectId = project.Id };
        await ApplyAsync(activity, project, request, ct);
        db.SchoolActivities.Add(activity);
        await db.SaveChangesAsync(ct);
        return await GetAsync(projectSlug, activity.Id, ct);
    }

    public async Task<SchoolActivityAdminDto> UpdateAsync(string projectSlug, Guid id, SchoolActivityUpsertRequest request, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var activity = await LoadAsync(project.Id, id, tracking: true, ct);
        if (request.ConcurrencyStamp is not { } stamp)
            throw ContentReferences.Invalid(nameof(request.ConcurrencyStamp), "The concurrency stamp is required when updating.");
        db.EnsureStamp(activity, stamp);
        await ApplyAsync(activity, project, request, ct);
        await db.SaveChangesAsync(ct);
        return await GetAsync(projectSlug, id, ct);
    }

    public async Task DeleteAsync(string projectSlug, Guid id, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var activity = await LoadAsync(project.Id, id, tracking: true, ct);
        db.SchoolActivities.Remove(activity);
        await db.SaveChangesAsync(ct);
    }

    private async Task ApplyAsync(SchoolActivity activity, ProjectRef project, SchoolActivityUpsertRequest request, CancellationToken ct)
    {
        await references.EnsureImageAsync(request.CoverImageId, nameof(request.CoverImageId), ct);
        var gallery = await references.GalleryAsync(request.GalleryImageIds, nameof(request.GalleryImageIds), ct);

        activity.Slug = await slugs.AssignAsync(
            SluggedContentType.SchoolActivity,
            project.Id,
            activity.Id,
            activity.Slug,
            request.Slug,
            request.Title.Ka ?? string.Empty,
            activity.Status == PublicationStatus.Published,
            candidate => db.SchoolActivities.AnyAsync(a => a.ProjectId == project.Id && a.Slug == candidate && a.Id != activity.Id, ct),
            ct);

        activity.Title.Apply(request.Title);
        activity.Summary.Apply(request.Summary);
        activity.Body.ApplyHtml(request.Body, sanitizer);
        activity.SchoolName.Apply(request.SchoolName);
        activity.Region.Apply(request.Region);
        activity.ActivityDate = request.ActivityDate;
        activity.CoverImageId = request.CoverImageId;
        activity.Status = request.Status;
        activity.MediaConsentConfirmed = request.MediaConsentConfirmed;
        activity.ConsentReference = TextUtils.NullIfBlank(request.ConsentReference);
        activity.Gallery.SyncOrdered(
            gallery,
            g => g.MediaAssetId,
            mediaId => new SchoolActivityGalleryItem { SchoolActivityId = activity.Id, MediaAssetId = mediaId },
            (g, order) => g.SortOrder = order);
    }

    private async Task<SchoolActivity> LoadAsync(Guid projectId, Guid id, bool tracking, CancellationToken ct)
    {
        var query = db.SchoolActivities
            .Include(a => a.CoverImage)
            .Include(a => a.Gallery).ThenInclude(g => g.MediaAsset)
            .AsSplitQuery();
        return await (tracking ? query : query.AsNoTracking()).FirstOrDefaultAsync(a => a.Id == id && a.ProjectId == projectId, ct)
            ?? throw new NotFoundException("School activity", id);
    }

    private SchoolActivityAdminDto ToDto(SchoolActivity a, ProjectRef project) => new(
        a.Id,
        a.Slug,
        a.Title.ToDto(),
        a.Summary.ToDto(),
        a.Body.ToDto(),
        a.SchoolName.ToDto(),
        a.Region.ToDto(),
        a.ActivityDate,
        a.CoverImage.ToRef(storage),
        a.Gallery.OrderBy(g => g.SortOrder).Select(g => g.MediaAsset.ToRef(storage)!).ToList(),
        a.Status,
        a.MediaConsentConfirmed,
        a.ConsentReference,
        PublicPaths.SchoolActivity(project.Slug, a.Slug),
        AuditInfo.Of(a));
}
