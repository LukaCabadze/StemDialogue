using Microsoft.EntityFrameworkCore;
using StemDialogue.Application.Abstractions;
using StemDialogue.Application.Common;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.Pages;
using StemDialogue.Domain.Projects;

namespace StemDialogue.Application.Projects;

/// <summary>Public read model of the project shell: header, funding strip, navigation and footer.</summary>
public sealed class SiteService(IAppDbContext db, ProjectResolver projects, PublicMapper map)
{
    public async Task<PlatformDto> GetPlatformAsync(CancellationToken ct)
    {
        var all = await db.Projects.AsNoTracking()
            .OrderByDescending(p => p.IsDefault).ThenByDescending(p => p.StartDate)
            .ToListAsync(ct);

        var summaries = all
            .Select(p => new ProjectSummaryDto(p.Slug, p.Code, map.Text(p.Name), p.Status, p.StartDate, p.EndDate, PublicPaths.Project(p.Slug)))
            .ToList();
        var defaultProject = all.FirstOrDefault(p => p.IsDefault) ?? all.FirstOrDefault();

        return new PlatformDto(defaultProject?.Slug, defaultProject is null ? null : PublicPaths.Project(defaultProject.Slug), summaries);
    }

    public async Task<SiteDto> GetSiteAsync(string projectSlug, CancellationToken ct)
    {
        var reference = await projects.ResolveAsync(projectSlug, ct);
        var project = await db.Projects.AsNoTracking()
            .Include(p => p.Logo)
            .Include(p => p.ShareImage)
            .Include(p => p.SocialLinks)
            .FirstAsync(p => p.Id == reference.Id, ct);

        var funding = await GetFundingAsync(project, ct);
        var navigation = await db.Pages.AsNoTracking()
            .Where(p => p.ProjectId == project.Id && p.ShowInNavigation)
            .OrderBy(p => p.NavigationOrder)
            .Select(p => new { p.Key, p.Title, p.NavigationLabel })
            .ToListAsync(ct);

        var name = map.Text(project.Name);
        var languages = project.Name.En is null
            ? new[] { LocalizedText.Georgian }
            : new[] { LocalizedText.Georgian, LocalizedText.English };

        return new SiteDto(
            project.Slug,
            project.Code,
            name,
            map.Text(project.ShortName),
            map.TextOrNull(project.Tagline),
            project.Status,
            project.StartDate,
            project.EndDate,
            map.Media(project.Logo),
            funding,
            new ContactDto(
                map.TextOrNull(project.ContactPerson),
                project.ContactEmail,
                project.ContactPhone,
                map.TextOrNull(project.ContactAddress)),
            project.SocialLinks.OrderBy(s => s.DisplayOrder).Select(s => new SocialLinkDto(s.Platform, s.Url)).ToList(),
            navigation
                .Where(p => PageKeys.IsValid(p.Key))
                .Select(p => new NavItemDto(p.Key, map.TextOrNull(p.NavigationLabel) ?? map.Text(p.Title), PublicPaths.Page(project.Slug, p.Key)))
                .ToList(),
            map.Seo(project, project.ShareImage, name, map.TextOrNull(project.Tagline) ?? map.Text(project.FundingFullText), project.Logo, PublicPaths.Project(project.Slug)),
            map.Language,
            languages);
    }

    /// <summary>Funding and partnership block (used by the layout, home and about pages).</summary>
    public async Task<FundingDto> GetFundingAsync(Guid projectId, CancellationToken ct)
    {
        var project = await db.Projects.AsNoTracking().FirstAsync(p => p.Id == projectId, ct);
        return await GetFundingAsync(project, ct);
    }

    private async Task<FundingDto> GetFundingAsync(Project project, CancellationToken ct)
    {
        var partners = await db.ProjectPartners.AsNoTracking()
            .Where(p => p.ProjectId == project.Id)
            .Include(p => p.Organization).ThenInclude(o => o.Logo)
            .Include(p => p.Organization).ThenInclude(o => o.LogoOnDark)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(ct);

        var mapped = partners.Select(ToDto).ToList();
        return new FundingDto(
            map.Text(project.FundingShortText),
            map.Text(project.FundingFullText),
            mapped.Where(p => p.Role == PartnerRole.Funder).ToList(),
            mapped.Where(p => p.Role == PartnerRole.PartnerUniversity).ToList(),
            mapped.Where(p => p.Role == PartnerRole.Supporter).ToList());
    }

    private PartnerDto ToDto(ProjectPartner partner)
    {
        var organization = partner.Organization;
        return new PartnerDto(
            organization.Id,
            partner.Role,
            map.TextOrNull(partner.RoleLabel) ?? EnumLabels.Of(partner.Role, map.Language),
            map.Text(organization.Name),
            map.Text(organization.ShortName),
            map.TextOrNull(organization.Description),
            organization.WebsiteUrl,
            map.Media(organization.Logo),
            map.Media(organization.LogoOnDark),
            partner.ShowInHeader);
    }
}
