using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StemDialogue.Application.Abstractions;
using StemDialogue.Application.Common;
using StemDialogue.Domain.Projects;

namespace StemDialogue.Application.Projects;

public sealed record PartnerInput(Guid OrganizationId, PartnerRole Role, LocalizedTextDto? RoleLabel, bool ShowInHeader = true);

public sealed record SocialLinkInput(SocialPlatform Platform, string Url);

/// <summary>Project settings editable from the CMS, including the header and footer funding wording (spec §2.1).</summary>
public sealed record ProjectUpdateRequest(
    string Code,
    LocalizedTextDto Name,
    LocalizedTextDto? ShortName,
    LocalizedTextDto? Tagline,
    ProjectStatus Status,
    DateOnly StartDate,
    DateOnly EndDate,
    LocalizedTextDto FundingShortText,
    LocalizedTextDto FundingFullText,
    Guid? LogoId,
    LocalizedTextDto? ContactPerson,
    string? ContactEmail,
    string? ContactPhone,
    LocalizedTextDto? ContactAddress,
    LocalizedTextDto? SeoTitle,
    LocalizedTextDto? SeoDescription,
    Guid? ShareImageId,
    IReadOnlyList<PartnerInput>? Partners,
    IReadOnlyList<SocialLinkInput>? SocialLinks,
    Guid ConcurrencyStamp);

public sealed record PartnerAdminDto(Guid OrganizationId, string OrganizationName, PartnerRole Role, LocalizedTextDto RoleLabel, bool ShowInHeader, int DisplayOrder);

public sealed record ProjectAdminDto(
    Guid Id,
    string Slug,
    string Code,
    LocalizedTextDto Name,
    LocalizedTextDto ShortName,
    LocalizedTextDto Tagline,
    ProjectStatus Status,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsDefault,
    LocalizedTextDto FundingShortText,
    LocalizedTextDto FundingFullText,
    AdminMediaRef? Logo,
    LocalizedTextDto ContactPerson,
    string? ContactEmail,
    string? ContactPhone,
    LocalizedTextDto ContactAddress,
    LocalizedTextDto SeoTitle,
    LocalizedTextDto SeoDescription,
    AdminMediaRef? ShareImage,
    IReadOnlyList<PartnerAdminDto> Partners,
    IReadOnlyList<SocialLinkInput> SocialLinks,
    AuditInfo Audit);

public sealed class ProjectUpdateValidator : AbstractValidator<ProjectUpdateRequest>
{
    public ProjectUpdateValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Name).RequiredText(Limits.Title);
        RuleFor(x => x.ShortName).OptionalText(Limits.Name);
        RuleFor(x => x.Tagline).OptionalText(Limits.Short);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must not be before the start date.");
        RuleFor(x => x.FundingShortText).RequiredText(200);
        RuleFor(x => x.FundingFullText).RequiredText(1000);
        RuleFor(x => x.ContactPerson).OptionalText(Limits.Name);
        RuleFor(x => x.ContactAddress).OptionalText(Limits.Short);
        RuleFor(x => x.ContactEmail).EmailAddress().MaximumLength(254).When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
        RuleFor(x => x.ContactPhone).MaximumLength(40);
        RuleFor(x => x.SeoTitle).OptionalText(Limits.SeoTitle);
        RuleFor(x => x.SeoDescription).OptionalText(Limits.SeoDescription);
        RuleFor(x => x.Partners)
            .Must(p => p is null || p.Select(x => x.OrganizationId).Distinct().Count() == p.Count)
            .WithMessage("An organization can be listed only once.");
        RuleForEach(x => x.Partners).ChildRules(p =>
        {
            p.RuleFor(x => x.Role).IsInEnum();
            p.RuleFor(x => x.RoleLabel).OptionalText(Limits.Name);
        });
        RuleForEach(x => x.SocialLinks).ChildRules(s =>
        {
            s.RuleFor(x => x.Platform).IsInEnum();
            s.RuleFor(x => x.Url).NotEmpty().HttpsUrl();
        });
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public sealed class ProjectAdminService(
    IAppDbContext db,
    ProjectResolver projects,
    ContentReferences references,
    IFileStorage storage)
{
    public async Task<IReadOnlyList<ProjectSummaryDto>> ListAsync(CancellationToken ct) =>
        (await db.Projects.AsNoTracking().OrderByDescending(p => p.StartDate).ToListAsync(ct))
        .Select(p => new ProjectSummaryDto(p.Slug, p.Code, p.Name.Ka, p.Status, p.StartDate, p.EndDate, PublicPaths.Project(p.Slug)))
        .ToList();

    public async Task<ProjectAdminDto> GetAsync(string slug, CancellationToken ct)
    {
        var reference = await projects.ResolveAsync(slug, ct);
        return ToDto(await LoadAsync(reference.Id, tracking: false, ct));
    }

    public async Task<ProjectAdminDto> UpdateAsync(string slug, ProjectUpdateRequest request, CancellationToken ct)
    {
        var reference = await projects.ResolveAsync(slug, ct);
        var project = await LoadAsync(reference.Id, tracking: true, ct);
        db.EnsureStamp(project, request.ConcurrencyStamp);

        await references.EnsureImageAsync(request.LogoId, nameof(request.LogoId), ct);
        await references.EnsureImageAsync(request.ShareImageId, nameof(request.ShareImageId), ct);

        var partners = request.Partners ?? [];
        var organizationIds = partners.Select(p => p.OrganizationId).ToList();
        var known = await db.Organizations.CountAsync(o => organizationIds.Contains(o.Id), ct);
        if (known != organizationIds.Count) throw ContentReferences.Invalid(nameof(request.Partners), "One or more organizations do not exist.");

        project.Code = request.Code.Trim();
        project.Name.Apply(request.Name);
        project.ShortName.Apply(request.ShortName);
        project.Tagline.Apply(request.Tagline);
        project.Status = request.Status;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.FundingShortText.Apply(request.FundingShortText);
        project.FundingFullText.Apply(request.FundingFullText);
        project.LogoId = request.LogoId;
        project.ContactPerson.Apply(request.ContactPerson);
        project.ContactEmail = TextUtils.NullIfBlank(request.ContactEmail);
        project.ContactPhone = TextUtils.NullIfBlank(request.ContactPhone);
        project.ContactAddress.Apply(request.ContactAddress);
        project.SeoTitle.Apply(request.SeoTitle);
        project.SeoDescription.Apply(request.SeoDescription);
        project.ShareImageId = request.ShareImageId;

        project.Partners.RemoveAll(p => !organizationIds.Contains(p.OrganizationId));
        for (var i = 0; i < partners.Count; i++)
        {
            var input = partners[i];
            var partner = project.Partners.FirstOrDefault(p => p.OrganizationId == input.OrganizationId);
            if (partner is null)
            {
                partner = new ProjectPartner { ProjectId = project.Id, OrganizationId = input.OrganizationId };
                project.Partners.Add(partner);
            }

            partner.Role = input.Role;
            partner.RoleLabel.Apply(input.RoleLabel);
            partner.ShowInHeader = input.ShowInHeader;
            partner.DisplayOrder = i;
        }

        project.SocialLinks.Clear();
        var links = request.SocialLinks ?? [];
        for (var i = 0; i < links.Count; i++)
            project.SocialLinks.Add(new SocialLink { ProjectId = project.Id, Platform = links[i].Platform, Url = links[i].Url.Trim(), DisplayOrder = i });

        await db.SaveChangesAsync(ct);
        return ToDto(await LoadAsync(project.Id, tracking: false, ct));
    }

    private async Task<Project> LoadAsync(Guid id, bool tracking, CancellationToken ct)
    {
        var query = db.Projects
            .Include(p => p.Logo)
            .Include(p => p.ShareImage)
            .Include(p => p.SocialLinks)
            .Include(p => p.Partners).ThenInclude(p => p.Organization)
            .AsSplitQuery();
        return await (tracking ? query : query.AsNoTracking()).FirstAsync(p => p.Id == id, ct);
    }

    private ProjectAdminDto ToDto(Project p) => new(
        p.Id,
        p.Slug,
        p.Code,
        p.Name.ToDto(),
        p.ShortName.ToDto(),
        p.Tagline.ToDto(),
        p.Status,
        p.StartDate,
        p.EndDate,
        p.IsDefault,
        p.FundingShortText.ToDto(),
        p.FundingFullText.ToDto(),
        p.Logo.ToRef(storage),
        p.ContactPerson.ToDto(),
        p.ContactEmail,
        p.ContactPhone,
        p.ContactAddress.ToDto(),
        p.SeoTitle.ToDto(),
        p.SeoDescription.ToDto(),
        p.ShareImage.ToRef(storage),
        p.Partners
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new PartnerAdminDto(x.OrganizationId, x.Organization.Name.Ka, x.Role, x.RoleLabel.ToDto(), x.ShowInHeader, x.DisplayOrder))
            .ToList(),
        p.SocialLinks.OrderBy(s => s.DisplayOrder).Select(s => new SocialLinkInput(s.Platform, s.Url)).ToList(),
        AuditInfo.Of(p));
}
