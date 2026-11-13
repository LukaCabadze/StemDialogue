using Microsoft.EntityFrameworkCore;
using StemDialogue.Application.Abstractions;
using StemDialogue.Application.Common;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.People;

namespace StemDialogue.Application.People;

/// <summary>
/// People are global; their roles are scoped to projects. One profile per person, any number of roles (spec §7.2).
/// </summary>
public sealed class PeopleAdminService(
    IAppDbContext db,
    ContentReferences references,
    SlugManager slugs,
    IHtmlContentSanitizer sanitizer,
    IFileStorage storage,
    TimeProvider clock)
{
    public async Task<PagedResult<PersonAdminListItem>> ListAsync(PeopleAdminQuery query, CancellationToken ct)
    {
        var items = db.People.AsNoTracking();
        if (query.Role is { } role) items = items.Where(p => p.Roles.Any(r => r.Role == role));
        if (query.Status is { } status) items = items.Where(p => p.Status == status);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            items = items.Where(p =>
                p.FullName.Ka.Contains(term) || (p.FullName.En != null && p.FullName.En.Contains(term)) ||
                p.Slug.Contains(term) || (p.Email != null && p.Email.Contains(term)));
        }

        var page = await items
            .OrderBy(p => p.FullName.Ka)
            .Include(p => p.Roles)
            .Include(p => p.Events)
            .AsSplitQuery()
            .ToPagedAsync(query, ct);

        return page.Map(p => new PersonAdminListItem(
            p.Id,
            p.Slug,
            p.FullName.Ka,
            TextUtils.NullIfBlank(p.Organization.Ka),
            p.Status,
            p.Roles.Select(r => r.Role).Distinct().OrderBy(r => r).ToList(),
            p.PhotoId is not null,
            p.Events.Count,
            p.UpdatedAtUtc,
            p.UpdatedBy));
    }

    public async Task<PersonAdminDto> GetAsync(Guid id, CancellationToken ct) =>
        await ToDtoAsync(await LoadAsync(id, tracking: false, ct), ct);

    public async Task<PersonAdminDto> CreateAsync(PersonUpsertRequest request, CancellationToken ct)
    {
        var person = new Person();
        await ApplyAsync(person, request, ct);
        db.People.Add(person);
        await db.SaveChangesAsync(ct);
        return await GetAsync(person.Id, ct);
    }

    public async Task<PersonAdminDto> UpdateAsync(Guid id, PersonUpsertRequest request, CancellationToken ct)
    {
        var person = await LoadAsync(id, tracking: true, ct);
        if (request.ConcurrencyStamp is not { } stamp)
            throw ContentReferences.Invalid(nameof(request.ConcurrencyStamp), "The concurrency stamp is required when updating.");
        db.EnsureStamp(person, stamp);

        await ApplyAsync(person, request, ct);
        await db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    /// <summary>Replaces the person's roles, e.g. moves a member between team sections or ends a role.</summary>
    public async Task<PersonAdminDto> UpdateRolesAsync(Guid id, PersonRolesRequest request, CancellationToken ct)
    {
        var person = await LoadAsync(id, tracking: true, ct);
        db.EnsureStamp(person, request.ConcurrencyStamp);
        await ApplyRolesAsync(person, request.Roles, ct);
        person.UpdatedAtUtc = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    /// <summary>
    /// Deleting is allowed only for unused profiles. Former members are kept: end their role instead (spec §7.3).
    /// </summary>
    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var person = await LoadAsync(id, tracking: true, ct);
        if (await db.EventParticipants.AnyAsync(p => p.PersonId == id, ct))
            throw new ConflictException("This person is linked to events. Set an end date on their role or archive the profile instead.");
        if (await db.Resources.AnyAsync(r => r.AuthorId == id, ct))
            throw new ConflictException("This person is the author of resources. Unlink them first.");
        if (person.Status == PublicationStatus.Published)
            throw new ConflictException("Archive the profile before deleting it.");

        db.People.Remove(person);
        await db.SaveChangesAsync(ct);
    }

    private async Task ApplyAsync(Person person, PersonUpsertRequest request, CancellationToken ct)
    {
        await references.EnsureImageAsync(request.PhotoId, nameof(request.PhotoId), ct);
        await references.EnsureImageAsync(request.ShareImageId, nameof(request.ShareImageId), ct);
        var disciplines = await references.DisciplinesAsync(request.DisciplineIds, ct);

        var wasPublic = person.Status == PublicationStatus.Published || person.CreatedAtUtc != default;
        person.Slug = await slugs.AssignAsync(
            SluggedContentType.Person,
            null,
            person.Id,
            person.Slug,
            request.Slug,
            request.FullName.Ka ?? string.Empty,
            wasPublic,
            candidate => db.People.AnyAsync(p => p.Slug == candidate && p.Id != person.Id, ct),
            ct);

        person.FullName.Apply(request.FullName);
        person.AcademicTitle.Apply(request.AcademicTitle);
        person.Position.Apply(request.Position);
        person.Organization.Apply(request.Organization);
        person.ShortBio.Apply(request.ShortBio);
        person.Biography.ApplyHtml(request.Biography, sanitizer);
        person.Contribution.Apply(request.Contribution);
        person.PhotoId = request.PhotoId;
        person.Email = TextUtils.NullIfBlank(request.Email);
        person.Status = request.Status;
        person.DisplayOrder = request.DisplayOrder;
        person.SeoTitle.Apply(request.SeoTitle);
        person.SeoDescription.Apply(request.SeoDescription);
        person.ShareImageId = request.ShareImageId;
        person.Disciplines.SyncById(disciplines);

        person.Links.Clear();
        var links = request.Links ?? [];
        for (var i = 0; i < links.Count; i++)
        {
            var link = new PersonLink { PersonId = person.Id, Type = links[i].Type, Url = links[i].Url.Trim(), SortOrder = i };
            link.Label.Apply(links[i].Label);
            person.Links.Add(link);
        }

        if (request.Roles is not null) await ApplyRolesAsync(person, request.Roles, ct);
    }

    private async Task ApplyRolesAsync(Person person, IReadOnlyList<PersonRoleInput> roles, CancellationToken ct)
    {
        var projectIds = roles.Select(r => r.ProjectId).Distinct().ToList();
        if (projectIds.Count > 0 && await db.Projects.CountAsync(p => projectIds.Contains(p.Id), ct) != projectIds.Count)
            throw ContentReferences.Invalid("Roles", "One or more projects do not exist.");

        // Keep existing rows (and their ids) for roles that remain.
        person.Roles.RemoveAll(existing => !roles.Any(r => r.ProjectId == existing.ProjectId && r.Role == existing.Role));
        foreach (var input in roles)
        {
            var role = person.Roles.FirstOrDefault(r => r.ProjectId == input.ProjectId && r.Role == input.Role);
            if (role is null)
            {
                role = new PersonRoleAssignment { PersonId = person.Id, ProjectId = input.ProjectId, Role = input.Role };
                person.Roles.Add(role);
            }

            role.Title.Apply(input.Title);
            role.StartDate = input.StartDate;
            role.EndDate = input.EndDate;
            role.DisplayOrder = input.DisplayOrder;
        }
    }

    private async Task<Person> LoadAsync(Guid id, bool tracking, CancellationToken ct)
    {
        var query = db.People
            .Include(p => p.Photo)
            .Include(p => p.ShareImage)
            .Include(p => p.Roles)
            .Include(p => p.Links)
            .Include(p => p.Disciplines)
            .AsSplitQuery();

        return await (tracking ? query : query.AsNoTracking()).FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Person", id);
    }

    private async Task<PersonAdminDto> ToDtoAsync(Person p, CancellationToken ct)
    {
        var today = TbilisiTime.Today(clock.GetUtcNow().UtcDateTime);
        var projectIds = p.Roles.Select(r => r.ProjectId).Distinct().ToList();
        var projectSlugs = await db.Projects.AsNoTracking()
            .Where(x => projectIds.Contains(x.Id))
            .Select(x => new Projects.ProjectRef(x.Id, x.Slug))
            .ToListAsync(ct);

        return new PersonAdminDto(
            p.Id,
            p.Slug,
            p.FullName.ToDto(),
            p.AcademicTitle.ToDto(),
            p.Position.ToDto(),
            p.Organization.ToDto(),
            p.ShortBio.ToDto(),
            p.Biography.ToDto(),
            p.Contribution.ToDto(),
            p.Photo.ToRef(storage),
            p.Email,
            p.Status,
            p.DisplayOrder,
            p.Roles
                .OrderBy(r => r.Role).ThenBy(r => r.DisplayOrder)
                .Select(r => new PersonRoleAdminDto(r.Id, r.ProjectId, r.Role, r.Title.ToDto(), r.StartDate, r.EndDate, r.DisplayOrder, r.IsCurrent(today)))
                .ToList(),
            p.Links.OrderBy(l => l.SortOrder).Select(l => new PersonLinkInput(l.Type, l.Url, l.Label.ToDto())).ToList(),
            p.Disciplines.Select(d => d.Id).ToList(),
            p.SeoTitle.ToDto(),
            p.SeoDescription.ToDto(),
            p.ShareImage.ToRef(storage),
            projectSlugs.Select(project => ContentCards.PersonPath(p, project)).ToList(),
            AuditInfo.Of(p));
    }
}
