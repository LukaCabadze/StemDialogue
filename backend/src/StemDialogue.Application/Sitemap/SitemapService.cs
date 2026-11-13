using Microsoft.EntityFrameworkCore;
using StemDialogue.Application.Abstractions;
using StemDialogue.Application.Common;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.Pages;

namespace StemDialogue.Application.Sitemap;

public sealed record SitemapEntry(string Path, DateTime LastModifiedUtc);

/// <summary>
/// All public URLs: feeds <c>/sitemap.xml</c> and the route list for Angular prerendering/SSG (spec §11).
/// </summary>
public sealed class SitemapService(IAppDbContext db, TimeProvider clock)
{
    public async Task<IReadOnlyList<SitemapEntry>> EntriesAsync(CancellationToken ct)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var entries = new List<SitemapEntry>();
        var projects = await db.Projects.AsNoTracking().Select(p => new { p.Id, p.Slug, p.UpdatedAtUtc }).ToListAsync(ct);

        foreach (var project in projects)
        {
            var pages = await db.Pages.AsNoTracking()
                .Where(p => p.ProjectId == project.Id)
                .Select(p => new { p.Key, p.UpdatedAtUtc })
                .ToListAsync(ct);
            entries.AddRange(pages.Where(p => PageKeys.IsValid(p.Key)).Select(p => new SitemapEntry(PublicPaths.Page(project.Slug, p.Key), p.UpdatedAtUtc)));

            entries.AddRange((await db.News.AsNoTracking().Live(project.Id, now).Select(n => new { n.Slug, n.UpdatedAtUtc }).ToListAsync(ct))
                .Select(n => new SitemapEntry(PublicPaths.News(project.Slug, n.Slug), n.UpdatedAtUtc)));

            entries.AddRange((await db.Events.AsNoTracking().Live(project.Id).Select(e => new { e.Slug, e.UpdatedAtUtc }).ToListAsync(ct))
                .Select(e => new SitemapEntry(PublicPaths.Event(project.Slug, e.Slug), e.UpdatedAtUtc)));

            entries.AddRange((await db.Resources.AsNoTracking().Live(project.Id, now).Select(r => new { r.Slug, r.UpdatedAtUtc }).ToListAsync(ct))
                .Select(r => new SitemapEntry(PublicPaths.Resource(project.Slug, r.Slug), r.UpdatedAtUtc)));

            entries.AddRange((await db.SchoolActivities.AsNoTracking()
                    .Where(a => a.ProjectId == project.Id && a.Status == PublicationStatus.Published)
                    .Select(a => new { a.Slug, a.UpdatedAtUtc }).ToListAsync(ct))
                .Select(a => new SitemapEntry(PublicPaths.SchoolActivity(project.Slug, a.Slug), a.UpdatedAtUtc)));

            var people = await db.People.AsNoTracking().Live(project.Id).Include(p => p.Roles).ToListAsync(ct);
            var reference = new Projects.ProjectRef(project.Id, project.Slug);
            entries.AddRange(people.Select(p => new SitemapEntry(ContentCards.PersonPath(p, reference), p.UpdatedAtUtc)));
        }

        return entries.DistinctBy(e => e.Path).ToList();
    }
}
