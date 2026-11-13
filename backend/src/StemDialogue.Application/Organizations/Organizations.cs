using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StemDialogue.Application.Abstractions;
using StemDialogue.Application.Common;
using StemDialogue.Domain.Organizations;

namespace StemDialogue.Application.Organizations;

public sealed record OrganizationUpsertRequest(
    LocalizedTextDto Name,
    LocalizedTextDto? ShortName,
    LocalizedTextDto? Description,
    string? WebsiteUrl,
    Guid? LogoId,
    Guid? LogoOnDarkId,
    Guid? ConcurrencyStamp);

public sealed record OrganizationAdminDto(
    Guid Id,
    LocalizedTextDto Name,
    LocalizedTextDto ShortName,
    LocalizedTextDto Description,
    string? WebsiteUrl,
    AdminMediaRef? Logo,
    AdminMediaRef? LogoOnDark,
    IReadOnlyList<string> UsedByProjects,
    AuditInfo Audit);

public sealed class OrganizationUpsertValidator : AbstractValidator<OrganizationUpsertRequest>
{
    public OrganizationUpsertValidator()
    {
        RuleFor(x => x.Name).RequiredText(Limits.Title);
        RuleFor(x => x.ShortName).OptionalText(40);
        RuleFor(x => x.Description).OptionalText(2000);
        RuleFor(x => x.WebsiteUrl).HttpsUrl();
    }
}

/// <summary>Funders, universities and other institutions with their official logos (spec §2, §10).</summary>
public sealed class OrganizationAdminService(IAppDbContext db, ContentReferences references, IFileStorage storage)
{
    public async Task<IReadOnlyList<OrganizationAdminDto>> ListAsync(CancellationToken ct)
    {
        var items = await Query(tracking: false).OrderBy(o => o.Name.Ka).ToListAsync(ct);
        var usage = await UsageAsync(ct);
        return items.Select(o => ToDto(o, usage)).ToList();
    }

    public async Task<OrganizationAdminDto> GetAsync(Guid id, CancellationToken ct) =>
        ToDto(await LoadAsync(id, tracking: false, ct), await UsageAsync(ct));

    public async Task<OrganizationAdminDto> CreateAsync(OrganizationUpsertRequest request, CancellationToken ct)
    {
        var organization = new Organization();
        await ApplyAsync(organization, request, ct);
        db.Organizations.Add(organization);
        await db.SaveChangesAsync(ct);
        return await GetAsync(organization.Id, ct);
    }

    public async Task<OrganizationAdminDto> UpdateAsync(Guid id, OrganizationUpsertRequest request, CancellationToken ct)
    {
        var organization = await LoadAsync(id, tracking: true, ct);
        if (request.ConcurrencyStamp is not { } stamp)
            throw ContentReferences.Invalid(nameof(request.ConcurrencyStamp), "The concurrency stamp is required when updating.");
        db.EnsureStamp(organization, stamp);
        await ApplyAsync(organization, request, ct);
        await db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var organization = await LoadAsync(id, tracking: true, ct);
        if (await db.ProjectPartners.AnyAsync(p => p.OrganizationId == id, ct))
            throw new ConflictException("This organization is a project partner. Remove it from the project first.");
        db.Organizations.Remove(organization);
        await db.SaveChangesAsync(ct);
    }

    private async Task ApplyAsync(Organization organization, OrganizationUpsertRequest request, CancellationToken ct)
    {
        await references.EnsureImageAsync(request.LogoId, nameof(request.LogoId), ct);
        await references.EnsureImageAsync(request.LogoOnDarkId, nameof(request.LogoOnDarkId), ct);
        organization.Name.Apply(request.Name);
        organization.ShortName.Apply(request.ShortName);
        organization.Description.Apply(request.Description);
        organization.WebsiteUrl = TextUtils.NullIfBlank(request.WebsiteUrl);
        organization.LogoId = request.LogoId;
        organization.LogoOnDarkId = request.LogoOnDarkId;
    }

    private IQueryable<Organization> Query(bool tracking)
    {
        var query = db.Organizations.Include(o => o.Logo).Include(o => o.LogoOnDark);
        return tracking ? query : query.AsNoTracking();
    }

    private async Task<Organization> LoadAsync(Guid id, bool tracking, CancellationToken ct) =>
        await Query(tracking).FirstOrDefaultAsync(o => o.Id == id, ct) ?? throw new NotFoundException("Organization", id);

    private async Task<ILookup<Guid, string>> UsageAsync(CancellationToken ct) =>
        (await db.ProjectPartners.AsNoTracking()
            .Join(db.Projects, p => p.ProjectId, p => p.Id, (partner, project) => new { partner.OrganizationId, project.Slug })
            .ToListAsync(ct))
        .ToLookup(x => x.OrganizationId, x => x.Slug);

    private OrganizationAdminDto ToDto(Organization o, ILookup<Guid, string> usage) => new(
        o.Id,
        o.Name.ToDto(),
        o.ShortName.ToDto(),
        o.Description.ToDto(),
        o.WebsiteUrl,
        o.Logo.ToRef(storage),
        o.LogoOnDark.ToRef(storage),
        usage[o.Id].ToList(),
        AuditInfo.Of(o));
}
