using Microsoft.EntityFrameworkCore;
using StemDialogue.Application.Abstractions;
using StemDialogue.Application.Common;
using StemDialogue.Application.Projects;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.News;

namespace StemDialogue.Application.News;

public sealed class PublicNewsService(
    IAppDbContext db,
    ProjectResolver projects,
    ContentCards cards,
    PublicMapper map,
    SlugManager slugs)
{
    public async Task<PagedResult<NewsCardDto>> ListAsync(string projectSlug, NewsQuery query, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var items = db.News.AsNoTracking().Live(project.Id, cards.UtcNow);

        if (query.Category is { } category) items = items.Where(n => n.Category == category);
        if (!string.IsNullOrWhiteSpace(query.Tag)) items = items.Where(n => n.Tags.Any(t => t.Slug == query.Tag));
        if (query.Year is >= 2000 and <= 2100)
        {
            var (from, to) = TbilisiTime.MonthRange(query.Year.Value, null);
            items = items.Where(n => n.PublishAtUtc >= from && n.PublishAtUtc < to);
        }

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var term = query.Q.Trim();
            items = items.Where(n =>
                n.Title.Ka.Contains(term) || (n.Title.En != null && n.Title.En.Contains(term)) ||
                n.Summary.Ka.Contains(term) || (n.Summary.En != null && n.Summary.En.Contains(term)));
        }

        var page = await items
            .OrderByDescending(n => n.PublishAtUtc)
            .ThenByDescending(n => n.CreatedAtUtc)
            .WithCardData()
            .ToPagedAsync(query, ct);

        return page.Map(n => cards.News(n, project));
    }

    /// <summary>Categories that currently have published articles, for the list filter.</summary>
    public async Task<IReadOnlyList<CategoryCountDto>> CategoriesAsync(string projectSlug, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var counts = await db.News.AsNoTracking()
            .Live(project.Id, cards.UtcNow)
            .GroupBy(n => n.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return counts
            .OrderBy(c => c.Category)
            .Select(c => new CategoryCountDto(c.Category, EnumLabels.Of(c.Category, map.Language), c.Count))
            .ToList();
    }

    public async Task<NewsDetailDto> GetAsync(string projectSlug, string slug, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var now = cards.UtcNow;

        var article = await db.News.AsNoTracking()
            .Live(project.Id, now)
            .Include(n => n.CoverImage)
            .Include(n => n.ShareImage)
            .Include(n => n.Tags)
            .Include(n => n.Gallery).ThenInclude(g => g.MediaAsset)
            .AsSplitQuery()
            .FirstOrDefaultAsync(n => n.Slug == slug, ct);

        if (article is null)
        {
            var targetId = await slugs.FindRedirectAsync(SluggedContentType.News, project.Id, slug, ct);
            var moved = targetId == Guid.Empty
                ? null
                : await db.News.AsNoTracking().Live(project.Id, now).Where(n => n.Id == targetId).Select(n => n.Slug).FirstOrDefaultAsync(ct);
            throw moved is null ? new NotFoundException("News", slug) : (Exception)new SlugMovedException(moved);
        }

        var relatedEvent = article.RelatedEventId is null
            ? null
            : await db.Events.AsNoTracking().Live(project.Id).WithCardData().FirstOrDefaultAsync(e => e.Id == article.RelatedEventId, ct);

        var related = await db.News.AsNoTracking()
            .Live(project.Id, now)
            .Where(n => n.Id != article.Id && n.Category == article.Category)
            .OrderByDescending(n => n.PublishAtUtc)
            .Take(3)
            .WithCardData()
            .ToListAsync(ct);

        var title = map.Text(article.Title);
        var summary = map.TextOrNull(article.Summary);
        var body = map.Text(article.Body);
        var path = PublicPaths.News(project.Slug, article.Slug);

        return new NewsDetailDto(
            article.Slug,
            title,
            summary ?? TextUtils.Excerpt(body, 220),
            body,
            article.Category,
            EnumLabels.Of(article.Category, map.Language),
            TbilisiTime.ToLocal(article.PublishAtUtc!.Value),
            TbilisiTime.ToLocal(article.UpdatedAtUtc),
            map.Media(article.CoverImage),
            article.Gallery.OrderBy(g => g.SortOrder).Select(g => map.Media(g.MediaAsset)!).ToList(),
            map.Terms(article.Tags),
            relatedEvent is null ? null : cards.Event(relatedEvent, project),
            related.Select(n => cards.News(n, project)).ToList(),
            map.Seo(article, article.ShareImage, title, summary ?? body, article.CoverImage, path),
            path);
    }
}
