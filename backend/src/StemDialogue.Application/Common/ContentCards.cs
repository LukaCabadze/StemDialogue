using Microsoft.EntityFrameworkCore;
using StemDialogue.Application.Events;
using StemDialogue.Application.News;
using StemDialogue.Application.People;
using StemDialogue.Application.Projects;
using StemDialogue.Application.Resources;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.Events;
using StemDialogue.Domain.News;
using StemDialogue.Domain.People;
using StemDialogue.Domain.Resources;

namespace StemDialogue.Application.Common;

/// <summary>Visibility rules of the public site, in one place.</summary>
public static class PublicFilters
{
    public static IQueryable<NewsArticle> Live(this IQueryable<NewsArticle> query, Guid projectId, DateTime nowUtc) =>
        query.Where(n => n.ProjectId == projectId && n.Status == PublicationStatus.Published && n.PublishAtUtc != null && n.PublishAtUtc <= nowUtc);

    public static IQueryable<Event> Live(this IQueryable<Event> query, Guid projectId) =>
        query.Where(e => e.ProjectId == projectId && e.Publication == PublicationStatus.Published);

    public static IQueryable<Resource> Live(this IQueryable<Resource> query, Guid projectId, DateTime nowUtc) =>
        query.Where(r => r.ProjectId == projectId && r.Status == PublicationStatus.Published && (r.PublishedAtUtc == null || r.PublishedAtUtc <= nowUtc));

    /// <summary>Published people holding at least one role in the project.</summary>
    public static IQueryable<Person> Live(this IQueryable<Person> query, Guid projectId) =>
        query.Where(p => p.Status == PublicationStatus.Published && p.Roles.Any(r => r.ProjectId == projectId));

    public static IQueryable<Event> WithCardData(this IQueryable<Event> query) =>
        query
            .Include(e => e.CoverImage)
            .Include(e => e.Tags)
            .Include(e => e.Disciplines)
            .Include(e => e.Resources)
            .Include(e => e.Participants).ThenInclude(p => p.Person).ThenInclude(p => p.Photo)
            .Include(e => e.Participants).ThenInclude(p => p.Person).ThenInclude(p => p.Roles)
            .AsSplitQuery();

    public static IQueryable<NewsArticle> WithCardData(this IQueryable<NewsArticle> query) =>
        query.Include(n => n.CoverImage).Include(n => n.Tags).AsSplitQuery();

    public static IQueryable<Resource> WithCardData(this IQueryable<Resource> query) =>
        query
            .Include(r => r.File)
            .Include(r => r.Thumbnail)
            .Include(r => r.Tags)
            .Include(r => r.Event)
            .Include(r => r.Author).ThenInclude(a => a!.Photo)
            .Include(r => r.Author).ThenInclude(a => a!.Roles)
            .AsSplitQuery();
}

/// <summary>Maps entities to the shared card models used across public pages.</summary>
public sealed class ContentCards(PublicMapper map, Abstractions.IFileStorage storage, TimeProvider clock)
{
    public string Language => map.Language;

    public DateTime UtcNow => clock.GetUtcNow().UtcDateTime;

    public NewsCardDto News(NewsArticle n, ProjectRef project) => new(
        n.Slug,
        map.Text(n.Title),
        map.TextOrNull(n.Summary) ?? TextUtils.Excerpt(map.Text(n.Body), 220),
        n.Category,
        EnumLabels.Of(n.Category, map.Language),
        TbilisiTime.ToLocal(n.PublishAtUtc ?? n.CreatedAtUtc),
        map.Media(n.CoverImage),
        map.Terms(n.Tags),
        n.IsFeatured,
        PublicPaths.News(project.Slug, n.Slug));

    public EventCardDto Event(Event e, ProjectRef project)
    {
        var now = UtcNow;
        var registrationOpen = IsRegistrationOpen(e, now);
        return new EventCardDto(
            e.Slug,
            map.Text(e.Title),
            map.TextOrNull(e.Summary),
            e.Type,
            EnumLabels.Of(e.Type, map.Language),
            e.Status,
            EnumLabels.Of(e.Status, map.Language),
            e.Format,
            EnumLabels.Of(e.Format, map.Language),
            TbilisiTime.ToLocal(e.StartsAtUtc),
            TbilisiTime.ToLocal(e.EndsAtUtc),
            map.TextOrNull(e.Venue),
            map.Media(e.CoverImage),
            Participants(e, project),
            map.Terms(e.Disciplines),
            map.Terms(e.Tags),
            e.IsUpcoming(now),
            !string.IsNullOrWhiteSpace(e.RecordingUrl),
            e.Resources.Any(r => r.Status == PublicationStatus.Published),
            registrationOpen ? e.RegistrationUrl : null,
            PublicPaths.Event(project.Slug, e.Slug));
    }

    public IReadOnlyList<PersonRefDto> Participants(Event e, ProjectRef project) =>
        e.Participants
            .Where(p => p.Person.Status == PublicationStatus.Published)
            .OrderBy(p => p.SortOrder)
            .Select(p => PersonRef(p.Person, project, EnumLabels.Of(p.Role, map.Language)))
            .ToList();

    public PersonRefDto PersonRef(Person person, ProjectRef project, string? role = null) => new(
        person.Slug,
        map.Text(person.FullName),
        map.TextOrNull(person.AcademicTitle),
        map.TextOrNull(person.Organization),
        map.Media(person.Photo),
        role,
        PersonPath(person, project));

    public ResourceCardDto Resource(Resource r, ProjectRef project)
    {
        var url = r.File is not null ? storage.GetPublicUrl(r.File.StoragePath) : r.ExternalUrl ?? string.Empty;
        return new ResourceCardDto(
            r.Slug,
            map.Text(r.Title),
            map.TextOrNull(r.Description),
            r.Type,
            EnumLabels.Of(r.Type, map.Language),
            url,
            r.Type == ResourceType.Video ? VideoEmbed.ToEmbedUrl(r.ExternalUrl) : null,
            map.Media(r.File),
            map.Media(r.Thumbnail),
            map.TextOrNull(r.AuthorName) ?? (r.Author is null ? null : map.Text(r.Author.FullName)),
            r.Author is { Status: PublicationStatus.Published } author ? PersonRef(author, project) : null,
            r.Event is { Publication: PublicationStatus.Published } ev
                ? new EventRefDto(ev.Slug, map.Text(ev.Title), TbilisiTime.ToLocal(ev.StartsAtUtc), PublicPaths.Event(project.Slug, ev.Slug))
                : null,
            TbilisiTime.ToLocal(r.PublishedAtUtc ?? r.CreatedAtUtc),
            map.Terms(r.Tags),
            PublicPaths.Resource(project.Slug, r.Slug));
    }

    /// <summary>
    /// Canonical profile URL: researchers first, then mentors, otherwise the team page.
    /// The same profile is reachable from every list it appears in.
    /// </summary>
    public static string PersonPath(Person person, ProjectRef project)
    {
        var roles = person.Roles.Where(r => r.ProjectId == project.Id).Select(r => r.Role).ToList();
        if (roles.Contains(PersonRole.Researcher)) return PublicPaths.Researcher(project.Slug, person.Slug);
        if (roles.Contains(PersonRole.Mentor)) return PublicPaths.Mentor(project.Slug, person.Slug);
        return PublicPaths.TeamMember(project.Slug, person.Slug);
    }

    public static bool IsRegistrationOpen(Event e, DateTime nowUtc) =>
        !string.IsNullOrWhiteSpace(e.RegistrationUrl)
        && e.Status is EventStatus.Planned or EventStatus.Postponed
        && (e.RegistrationDeadlineUtc ?? e.StartsAtUtc) > nowUtc;
}
