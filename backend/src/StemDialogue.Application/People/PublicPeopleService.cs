using Microsoft.EntityFrameworkCore;
using StemDialogue.Application.Abstractions;
using StemDialogue.Application.Common;
using StemDialogue.Application.Events;
using StemDialogue.Application.Pages;
using StemDialogue.Application.Projects;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.Events;
using StemDialogue.Domain.Pages;
using StemDialogue.Domain.People;

namespace StemDialogue.Application.People;

/// <summary>Team page, researcher and mentor lists, and profiles (spec §7).</summary>
public sealed class PublicPeopleService(
    IAppDbContext db,
    ProjectResolver projects,
    ContentCards cards,
    PublicMapper map,
    PublicPageService pages,
    SlugManager slugs)
{
    private static readonly PersonRole[] TeamSections =
    [
        PersonRole.CoreTeam, PersonRole.Consultant, PersonRole.TechnologyTeam, PersonRole.OrganizationalStaff,
    ];

    public async Task<TeamDto> TeamAsync(string projectSlug, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var today = TbilisiTime.Today(cards.UtcNow);
        var page = await pages.FindAsync(project, PageKeys.Team, ct);

        var people = await db.People.AsNoTracking()
            .Where(p => p.Status == PublicationStatus.Published)
            .Where(p => p.Roles.Any(r => r.ProjectId == project.Id && TeamSections.Contains(r.Role)))
            .Include(p => p.Photo)
            .Include(p => p.Roles)
            .AsSplitQuery()
            .ToListAsync(ct);

        var sections = new List<TeamSectionDto>();
        foreach (var role in TeamSections)
        {
            var members = people
                .SelectMany(p => p.Roles.Where(r => r.ProjectId == project.Id && r.Role == role).Select(r => (Person: p, Role: r)))
                .OrderBy(x => x.Role.DisplayOrder).ThenBy(x => x.Person.DisplayOrder).ThenBy(x => x.Person.FullName.Ka)
                .ToList();
            if (members.Count == 0) continue;

            TeamMemberDto Member((Person Person, PersonRoleAssignment Role) x) => new(
                x.Person.Slug,
                map.Text(x.Person.FullName),
                map.TextOrNull(x.Role.Title),
                map.TextOrNull(x.Person.AcademicTitle),
                map.TextOrNull(x.Person.Organization),
                map.TextOrNull(x.Person.ShortBio),
                map.Media(x.Person.Photo),
                x.Role.StartDate,
                x.Role.EndDate,
                ContentCards.PersonPath(x.Person, project));

            sections.Add(new TeamSectionDto(
                role,
                EnumLabels.Of(role, map.Language),
                role == PersonRole.CoreTeam,
                members.Where(x => x.Role.IsCurrent(today)).Select(Member).ToList(),
                members.Where(x => !x.Role.IsCurrent(today)).Select(Member).ToList()));
        }

        return new TeamDto(page, sections);
    }

    public Task<PagedResult<PersonCardDto>> ResearchersAsync(string projectSlug, PeopleQuery query, CancellationToken ct) =>
        ListByRoleAsync(projectSlug, PersonRole.Researcher, query, ct);

    public Task<PagedResult<PersonCardDto>> MentorsAsync(string projectSlug, PeopleQuery query, CancellationToken ct) =>
        ListByRoleAsync(projectSlug, PersonRole.Mentor, query, ct);

    public async Task<PersonDetailDto> GetAsync(string projectSlug, string slug, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var now = cards.UtcNow;
        var today = TbilisiTime.Today(now);

        var person = await db.People.AsNoTracking()
            .Live(project.Id)
            .Include(p => p.Photo)
            .Include(p => p.ShareImage)
            .Include(p => p.Roles)
            .Include(p => p.Links)
            .Include(p => p.Disciplines)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Slug == slug, ct);

        if (person is null)
        {
            var targetId = await slugs.FindRedirectAsync(SluggedContentType.Person, null, slug, ct);
            var moved = targetId == Guid.Empty
                ? null
                : await db.People.AsNoTracking().Live(project.Id).Where(p => p.Id == targetId).Select(p => p.Slug).FirstOrDefaultAsync(ct);
            throw moved is null ? new NotFoundException("Person", slug) : (Exception)new SlugMovedException(moved);
        }

        // Related events are derived from event participation (spec §15.16).
        var events = await db.Events.AsNoTracking()
            .Live(project.Id)
            .Where(e => e.Participants.Any(p => p.PersonId == person.Id))
            .OrderByDescending(e => e.StartsAtUtc)
            .WithCardData()
            .ToListAsync(ct);

        var resources = await db.Resources.AsNoTracking()
            .Live(project.Id, now)
            .Where(r => r.AuthorId == person.Id)
            .OrderByDescending(r => r.PublishedAtUtc)
            .WithCardData()
            .ToListAsync(ct);

        var name = map.Text(person.FullName);
        var path = ContentCards.PersonPath(person, project);
        var roles = person.Roles
            .Where(r => r.ProjectId == project.Id)
            .OrderBy(r => r.Role)
            .Select(r => new PersonRoleDto(r.Role, EnumLabels.Of(r.Role, map.Language), map.TextOrNull(r.Title), r.StartDate, r.EndDate, r.IsCurrent(today)))
            .ToList();

        return new PersonDetailDto(
            person.Slug,
            name,
            map.TextOrNull(person.AcademicTitle),
            map.TextOrNull(person.Position),
            map.TextOrNull(person.Organization),
            map.TextOrNull(person.ShortBio),
            map.TextOrNull(person.Biography),
            map.TextOrNull(person.Contribution),
            map.Media(person.Photo),
            roles,
            person.Links.OrderBy(l => l.SortOrder).Select(l => new PersonLinkDto(l.Type, l.Url, map.TextOrNull(l.Label))).ToList(),
            map.Terms(person.Disciplines),
            events.Where(e => e.IsUpcoming(now)).OrderBy(e => e.StartsAtUtc).Select(e => cards.Event(e, project)).ToList(),
            events.Where(e => !e.IsUpcoming(now) && e.Status != EventStatus.Cancelled).Select(e => cards.Event(e, project)).ToList(),
            resources.Select(r => cards.Resource(r, project)).ToList(),
            map.Seo(person, person.ShareImage, name, map.TextOrNull(person.ShortBio) ?? map.Text(person.Biography), person.Photo, path),
            path);
    }

    private async Task<PagedResult<PersonCardDto>> ListByRoleAsync(string projectSlug, PersonRole role, PeopleQuery query, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var today = TbilisiTime.Today(cards.UtcNow);

        var items = db.People.AsNoTracking()
            .Where(p => p.Status == PublicationStatus.Published)
            .Where(p => p.Roles.Any(r => r.ProjectId == project.Id && r.Role == role));

        if (!query.IncludeFormer)
            items = items.Where(p => p.Roles.Any(r => r.ProjectId == project.Id && r.Role == role && (r.EndDate == null || r.EndDate >= today)));
        if (!string.IsNullOrWhiteSpace(query.Discipline))
            items = items.Where(p => p.Disciplines.Any(d => d.Slug == query.Discipline));
        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var term = query.Q.Trim();
            items = items.Where(p =>
                p.FullName.Ka.Contains(term) || (p.FullName.En != null && p.FullName.En.Contains(term)) ||
                p.Organization.Ka.Contains(term) || (p.Organization.En != null && p.Organization.En.Contains(term)));
        }

        var page = await items
            .OrderBy(p => p.DisplayOrder).ThenBy(p => p.FullName.Ka)
            .Include(p => p.Photo)
            .Include(p => p.Roles)
            .Include(p => p.Disciplines)
            .AsSplitQuery()
            .ToPagedAsync(query, ct);

        return page.Map(p =>
        {
            var projectRoles = p.Roles.Where(r => r.ProjectId == project.Id).ToList();
            return new PersonCardDto(
                p.Slug,
                map.Text(p.FullName),
                map.TextOrNull(p.AcademicTitle),
                map.TextOrNull(p.Position),
                map.TextOrNull(p.Organization),
                map.TextOrNull(p.ShortBio),
                map.Media(p.Photo),
                projectRoles.Select(r => r.Role).Distinct().OrderBy(r => r).ToList(),
                map.Terms(p.Disciplines),
                projectRoles.Any(r => r.Role == role && r.IsCurrent(today)),
                role == PersonRole.Mentor ? PublicPaths.Mentor(project.Slug, p.Slug) : PublicPaths.Researcher(project.Slug, p.Slug));
        });
    }
}
