using Microsoft.EntityFrameworkCore;
using StemDialogue.Application.Abstractions;
using StemDialogue.Application.Common;
using StemDialogue.Application.Events;
using StemDialogue.Application.News;
using StemDialogue.Application.Pages;
using StemDialogue.Application.Projects;
using StemDialogue.Application.Resources;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.Events;
using StemDialogue.Domain.Pages;
using StemDialogue.Domain.People;

namespace StemDialogue.Application.Home;

/// <summary>
/// Home page blocks from spec §4. Empty blocks come back as <c>null</c> or empty lists so the UI can hide
/// them instead of rendering empty sections.
/// </summary>
public sealed record HomeDto(
    PageDto? Hero,
    AboutTeaserDto? About,
    FeaturedDto? Featured,
    IReadOnlyList<EventCardDto> UpcomingEvents,
    IReadOnlyList<NewsCardDto> LatestNews,
    ArchiveHighlightsDto Archive,
    IReadOnlyList<PeopleEntryDto> People,
    AmbassadorsTeaserDto? Ambassadors,
    FundingDto Funding,
    SeoDto Seo);

public sealed record AboutTeaserDto(string Title, string? Lead, string Path, IReadOnlyList<MilestoneDto> Milestones);

/// <summary>Current highlight: a featured announcement or the next meeting.</summary>
public sealed record FeaturedDto(string Kind, NewsCardDto? News, EventCardDto? Event);

public sealed record ArchiveHighlightsDto(IReadOnlyList<EventCardDto> RecentEvents, IReadOnlyList<ResourceCardDto> Resources)
{
    public bool IsEmpty => RecentEvents.Count == 0 && Resources.Count == 0;
}

public sealed record PeopleEntryDto(string Key, string Title, string? Lead, int Count, IReadOnlyList<MediaDto> Photos, string Path);

public sealed record AmbassadorsTeaserDto(string Title, string? Lead, int ActivityCount, string Path);

public sealed class HomeService(
    IAppDbContext db,
    ProjectResolver projects,
    ContentCards cards,
    PublicMapper map,
    PublicPageService pages,
    SiteService site)
{
    private const int UpcomingCount = 3;
    private const int NewsCount = 4;

    public async Task<HomeDto> GetAsync(string projectSlug, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var now = cards.UtcNow;

        var hero = await pages.FindAsync(project, PageKeys.Home, ct);
        var about = await pages.FindAsync(project, PageKeys.About, ct);
        var ambassadors = await pages.FindAsync(project, PageKeys.StemAmbassadors, ct);

        var upcoming = await db.Events.AsNoTracking().Live(project.Id)
            .Where(e => e.Status != EventStatus.Completed && e.Status != EventStatus.Cancelled && (e.EndsAtUtc ?? e.StartsAtUtc) >= now)
            .OrderBy(e => e.StartsAtUtc)
            .Take(UpcomingCount)
            .WithCardData()
            .ToListAsync(ct);

        var latestNews = await db.News.AsNoTracking().Live(project.Id, now)
            .OrderByDescending(n => n.PublishAtUtc)
            .Take(NewsCount)
            .WithCardData()
            .ToListAsync(ct);

        var featured = await FeaturedAsync(project, upcoming, now, ct);

        var recentEvents = await db.Events.AsNoTracking().Live(project.Id)
            .Where(e => e.Status == EventStatus.Completed)
            .OrderByDescending(e => e.StartsAtUtc)
            .Take(3)
            .WithCardData()
            .ToListAsync(ct);

        var resources = await db.Resources.AsNoTracking().Live(project.Id, now)
            .OrderByDescending(r => r.IsFeatured)
            .ThenByDescending(r => r.PublishedAtUtc ?? r.CreatedAtUtc)
            .Take(4)
            .WithCardData()
            .ToListAsync(ct);

        var activityCount = await db.SchoolActivities.CountAsync(a => a.ProjectId == project.Id && a.Status == PublicationStatus.Published, ct);

        return new HomeDto(
            hero,
            about is null ? null : new AboutTeaserDto(about.Title, about.Lead, about.Path, await pages.MilestonesAsync(project, ct)),
            featured,
            upcoming.Select(e => cards.Event(e, project)).ToList(),
            latestNews.Select(n => cards.News(n, project)).ToList(),
            new ArchiveHighlightsDto(
                recentEvents.Select(e => cards.Event(e, project)).ToList(),
                resources.Select(r => cards.Resource(r, project)).ToList()),
            await PeopleAsync(project, ct),
            ambassadors is null ? null : new AmbassadorsTeaserDto(ambassadors.Title, ambassadors.Lead, activityCount, ambassadors.Path),
            await site.GetFundingAsync(project.Id, ct),
            hero?.Seo ?? new SeoDto(projectSlug, string.Empty, null, PublicPaths.Project(project.Slug)));
    }

    private async Task<FeaturedDto?> FeaturedAsync(ProjectRef project, List<Event> upcoming, DateTime now, CancellationToken ct)
    {
        var news = await db.News.AsNoTracking().Live(project.Id, now)
            .Where(n => n.IsFeatured)
            .OrderByDescending(n => n.PublishAtUtc)
            .WithCardData()
            .FirstOrDefaultAsync(ct);
        if (news is not null) return new FeaturedDto("news", cards.News(news, project), null);

        var featuredEvent = await db.Events.AsNoTracking().Live(project.Id)
            .Where(e => e.IsFeatured && e.Status != EventStatus.Cancelled && (e.EndsAtUtc ?? e.StartsAtUtc) >= now)
            .OrderBy(e => e.StartsAtUtc)
            .WithCardData()
            .FirstOrDefaultAsync(ct);
        var next = featuredEvent ?? upcoming.FirstOrDefault();
        return next is null ? null : new FeaturedDto("event", null, cards.Event(next, project));
    }

    private async Task<IReadOnlyList<PeopleEntryDto>> PeopleAsync(ProjectRef project, CancellationToken ct)
    {
        var entries = new List<PeopleEntryDto>();
        var groups = new (string Key, PersonRole[] Roles)[]
        {
            (PageKeys.Team, new[] { PersonRole.CoreTeam, PersonRole.Consultant, PersonRole.TechnologyTeam, PersonRole.OrganizationalStaff }),
            (PageKeys.Researchers, new[] { PersonRole.Researcher }),
            (PageKeys.Mentors, new[] { PersonRole.Mentor }),
        };

        foreach (var (key, roles) in groups)
        {
            var people = db.People.AsNoTracking()
                .Where(p => p.Status == PublicationStatus.Published && p.Roles.Any(r => r.ProjectId == project.Id && roles.Contains(r.Role)));
            var count = await people.CountAsync(ct);
            if (count == 0) continue;

            var photos = await people
                .Where(p => p.Photo != null)
                .OrderBy(p => p.DisplayOrder)
                .Take(4)
                .Select(p => p.Photo!)
                .ToListAsync(ct);
            var page = await pages.FindAsync(project, key, ct);

            entries.Add(new PeopleEntryDto(
                key,
                page?.Title ?? key,
                page?.Lead,
                count,
                photos.Select(p => map.Media(p)!).ToList(),
                PublicPaths.Page(project.Slug, key)));
        }

        return entries;
    }
}
