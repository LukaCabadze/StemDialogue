using Microsoft.EntityFrameworkCore;
using StemDialogue.Application.Abstractions;
using StemDialogue.Application.Common;
using StemDialogue.Application.Projects;
using StemDialogue.Domain.Pages;
using StemDialogue.Domain.Projects;

namespace StemDialogue.Application.Pages;

public sealed class PublicPageService(
    IAppDbContext db,
    ProjectResolver projects,
    PublicMapper map,
    SiteService site,
    TimeProvider clock)
{
    public async Task<PageDto> GetAsync(string projectSlug, string key, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        return await FindAsync(project, key, ct) ?? throw new NotFoundException("Page", key);
    }

    public async Task<PageDto?> FindAsync(ProjectRef project, string key, CancellationToken ct)
    {
        if (!PageKeys.IsValid(key)) return null;
        var page = await db.Pages.AsNoTracking()
            .Include(p => p.HeroImage)
            .Include(p => p.ShareImage)
            .FirstOrDefaultAsync(p => p.ProjectId == project.Id && p.Key == key, ct);
        return page is null ? null : ToDto(page, project);
    }

    /// <summary>About page: goals, project stages and the extended funder/partners block (spec §2.1, §3).</summary>
    public async Task<AboutDto> AboutAsync(string projectSlug, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var page = await FindAsync(project, PageKeys.About, ct) ?? throw new NotFoundException("Page", PageKeys.About);
        return new AboutDto(page, await MilestonesAsync(project, ct), await site.GetFundingAsync(project.Id, ct));
    }

    public async Task<ContactPageDto> ContactAsync(string projectSlug, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var page = await FindAsync(project, PageKeys.Contact, ct) ?? throw new NotFoundException("Page", PageKeys.Contact);
        var entity = await db.Projects.AsNoTracking().Include(p => p.SocialLinks).FirstAsync(p => p.Id == project.Id, ct);

        return new ContactPageDto(
            page,
            new ContactDto(map.TextOrNull(entity.ContactPerson), entity.ContactEmail, entity.ContactPhone, map.TextOrNull(entity.ContactAddress)),
            entity.SocialLinks.OrderBy(s => s.DisplayOrder).Select(s => new SocialLinkDto(s.Platform, s.Url)).ToList(),
            await site.GetFundingAsync(project.Id, ct));
    }

    public async Task<IReadOnlyList<MilestoneDto>> MilestonesAsync(ProjectRef project, CancellationToken ct)
    {
        var today = TbilisiTime.Today(clock.GetUtcNow().UtcDateTime);
        var milestones = await db.Milestones.AsNoTracking()
            .Where(m => m.ProjectId == project.Id && m.IsVisible)
            .OrderBy(m => m.DisplayOrder).ThenBy(m => m.StartDate)
            .ToListAsync(ct);
        return milestones.Select(m => ToDto(m, today)).ToList();
    }

    public PageDto ToDto(SitePage page, ProjectRef project)
    {
        var title = map.Text(page.Title);
        var lead = map.TextOrNull(page.Lead);
        var path = PublicPaths.Page(project.Slug, page.Key);
        return new PageDto(
            page.Key,
            title,
            lead,
            map.TextOrNull(page.Body),
            map.Media(page.HeroImage),
            Action(page.PrimaryActionLabel, page.PrimaryActionUrl),
            Action(page.SecondaryActionLabel, page.SecondaryActionUrl),
            map.Seo(page, page.ShareImage, title, lead ?? map.Text(page.Body), page.HeroImage, path),
            path);
    }

    private ActionDto? Action(Domain.Common.LocalizedText label, string? url) =>
        map.TextOrNull(label) is { } text && !string.IsNullOrWhiteSpace(url) ? new ActionDto(text, url) : null;

    private MilestoneDto ToDto(ProjectMilestone m, DateOnly today)
    {
        var state = m.EndDate is { } end && end < today
            ? MilestoneState.Done
            : m.StartDate is { } start && start <= today ? MilestoneState.Current : MilestoneState.Upcoming;
        if (m.StartDate is null && m.EndDate is null) state = MilestoneState.Upcoming;

        return new MilestoneDto(
            m.Id,
            map.Text(m.Title),
            map.TextOrNull(m.Description),
            map.TextOrNull(m.PeriodLabel),
            map.TextOrNull(m.Location),
            m.StartDate,
            m.EndDate,
            state);
    }
}

public sealed class PageAdminService(
    IAppDbContext db,
    ProjectResolver projects,
    ContentReferences references,
    IHtmlContentSanitizer sanitizer,
    IFileStorage storage)
{
    public async Task<IReadOnlyList<PageAdminListItem>> ListAsync(string projectSlug, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var pages = await db.Pages.AsNoTracking().Where(p => p.ProjectId == project.Id).OrderBy(p => p.NavigationOrder).ToListAsync(ct);
        return pages
            .Select(p => new PageAdminListItem(p.Key, p.Title.Ka, p.ShowInNavigation, p.NavigationOrder, p.UpdatedAtUtc, p.UpdatedBy, PublicPaths.Page(project.Slug, p.Key)))
            .ToList();
    }

    public async Task<PageAdminDto> GetAsync(string projectSlug, string key, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        return ToDto(await LoadAsync(project.Id, key, tracking: false, ct), project);
    }

    public async Task<PageAdminDto> UpdateAsync(string projectSlug, string key, PageUpdateRequest request, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var page = await LoadAsync(project.Id, key, tracking: true, ct);
        db.EnsureStamp(page, request.ConcurrencyStamp);

        await references.EnsureImageAsync(request.HeroImageId, nameof(request.HeroImageId), ct);
        await references.EnsureImageAsync(request.ShareImageId, nameof(request.ShareImageId), ct);

        page.Title.Apply(request.Title);
        page.NavigationLabel.Apply(request.NavigationLabel);
        page.Lead.Apply(request.Lead);
        page.Body.ApplyHtml(request.Body, sanitizer);
        page.HeroImageId = request.HeroImageId;
        page.PrimaryActionLabel.Apply(request.PrimaryActionLabel);
        page.PrimaryActionUrl = TextUtils.NullIfBlank(request.PrimaryActionUrl);
        page.SecondaryActionLabel.Apply(request.SecondaryActionLabel);
        page.SecondaryActionUrl = TextUtils.NullIfBlank(request.SecondaryActionUrl);
        page.ShowInNavigation = key == PageKeys.Home || request.ShowInNavigation;
        page.NavigationOrder = request.NavigationOrder;
        page.SeoTitle.Apply(request.SeoTitle);
        page.SeoDescription.Apply(request.SeoDescription);
        page.ShareImageId = request.ShareImageId;

        await db.SaveChangesAsync(ct);
        return ToDto(await LoadAsync(project.Id, key, tracking: false, ct), project);
    }

    private async Task<SitePage> LoadAsync(Guid projectId, string key, bool tracking, CancellationToken ct)
    {
        var query = db.Pages.Include(p => p.HeroImage).Include(p => p.ShareImage);
        return await (tracking ? query : query.AsNoTracking()).FirstOrDefaultAsync(p => p.ProjectId == projectId && p.Key == key, ct)
            ?? throw new NotFoundException("Page", key);
    }

    private PageAdminDto ToDto(SitePage p, ProjectRef project) => new(
        p.Id,
        p.Key,
        p.Title.ToDto(),
        p.NavigationLabel.ToDto(),
        p.Lead.ToDto(),
        p.Body.ToDto(),
        p.HeroImage.ToRef(storage),
        p.PrimaryActionLabel.ToDto(),
        p.PrimaryActionUrl,
        p.SecondaryActionLabel.ToDto(),
        p.SecondaryActionUrl,
        p.ShowInNavigation,
        p.NavigationOrder,
        p.SeoTitle.ToDto(),
        p.SeoDescription.ToDto(),
        p.ShareImage.ToRef(storage),
        PublicPaths.Page(project.Slug, p.Key),
        AuditInfo.Of(p));
}

public sealed class MilestoneAdminService(IAppDbContext db, ProjectResolver projects)
{
    public async Task<IReadOnlyList<MilestoneAdminDto>> ListAsync(string projectSlug, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var items = await db.Milestones.AsNoTracking().Where(m => m.ProjectId == project.Id).OrderBy(m => m.DisplayOrder).ToListAsync(ct);
        return items.Select(ToDto).ToList();
    }

    public async Task<MilestoneAdminDto> CreateAsync(string projectSlug, MilestoneUpsertRequest request, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var nextOrder = await db.Milestones.Where(m => m.ProjectId == project.Id).Select(m => (int?)m.DisplayOrder).MaxAsync(ct) ?? -1;
        var milestone = new ProjectMilestone { ProjectId = project.Id, DisplayOrder = nextOrder + 1 };
        Apply(milestone, request);
        db.Milestones.Add(milestone);
        await db.SaveChangesAsync(ct);
        return ToDto(milestone);
    }

    public async Task<MilestoneAdminDto> UpdateAsync(string projectSlug, Guid id, MilestoneUpsertRequest request, CancellationToken ct)
    {
        var milestone = await LoadAsync(projectSlug, id, ct);
        if (request.ConcurrencyStamp is not { } stamp)
            throw ContentReferences.Invalid(nameof(request.ConcurrencyStamp), "The concurrency stamp is required when updating.");
        db.EnsureStamp(milestone, stamp);
        Apply(milestone, request);
        await db.SaveChangesAsync(ct);
        return ToDto(milestone);
    }

    public async Task DeleteAsync(string projectSlug, Guid id, CancellationToken ct)
    {
        db.Milestones.Remove(await LoadAsync(projectSlug, id, ct));
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<MilestoneAdminDto>> ReorderAsync(string projectSlug, ReorderRequest request, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var items = await db.Milestones.Where(m => m.ProjectId == project.Id).ToListAsync(ct);
        if (items.Count != request.Ids.Count || items.Any(m => !request.Ids.Contains(m.Id)))
            throw ContentReferences.Invalid(nameof(request.Ids), "Send every milestone id exactly once.");

        foreach (var item in items) item.DisplayOrder = request.Ids.ToList().IndexOf(item.Id);
        await db.SaveChangesAsync(ct);
        return items.OrderBy(m => m.DisplayOrder).Select(ToDto).ToList();
    }

    private async Task<ProjectMilestone> LoadAsync(string projectSlug, Guid id, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        return await db.Milestones.FirstOrDefaultAsync(m => m.Id == id && m.ProjectId == project.Id, ct)
            ?? throw new NotFoundException("Milestone", id);
    }

    private static void Apply(ProjectMilestone m, MilestoneUpsertRequest r)
    {
        m.Title.Apply(r.Title);
        m.Description.Apply(r.Description);
        m.PeriodLabel.Apply(r.PeriodLabel);
        m.Location.Apply(r.Location);
        m.StartDate = r.StartDate;
        m.EndDate = r.EndDate;
        m.IsVisible = r.IsVisible;
    }

    private static MilestoneAdminDto ToDto(ProjectMilestone m) => new(
        m.Id, m.Title.ToDto(), m.Description.ToDto(), m.PeriodLabel.ToDto(), m.Location.ToDto(),
        m.StartDate, m.EndDate, m.DisplayOrder, m.IsVisible, AuditInfo.Of(m));
}
