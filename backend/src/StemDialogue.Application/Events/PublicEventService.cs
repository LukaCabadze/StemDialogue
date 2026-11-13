using Microsoft.EntityFrameworkCore;
using StemDialogue.Application.Abstractions;
using StemDialogue.Application.Common;
using StemDialogue.Application.Projects;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.Events;

namespace StemDialogue.Application.Events;

/// <summary>STEM Saturdays: upcoming meetings and the chronological archive (spec §6).</summary>
public sealed class PublicEventService(
    IAppDbContext db,
    ProjectResolver projects,
    ContentCards cards,
    PublicMapper map,
    SlugManager slugs)
{
    public async Task<PagedResult<EventCardDto>> ListAsync(string projectSlug, EventQuery query, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var now = cards.UtcNow;
        var items = db.Events.AsNoTracking().Live(project.Id);

        items = query.Scope switch
        {
            // Upcoming: not yet held. Cancelled future events stay listed with their status (old link kept).
            EventScope.Upcoming => items.Where(e => e.Status != EventStatus.Completed && (e.EndsAtUtc ?? e.StartsAtUtc) >= now),
            EventScope.Past => items.Where(e => e.Status == EventStatus.Completed || (e.EndsAtUtc ?? e.StartsAtUtc) < now),
            _ => items,
        };

        if (query.Year is >= 2000 and <= 2100)
        {
            var month = query.Month is >= 1 and <= 12 ? query.Month : null;
            var (from, to) = TbilisiTime.MonthRange(query.Year.Value, month);
            items = items.Where(e => e.StartsAtUtc >= from && e.StartsAtUtc < to);
        }

        if (!string.IsNullOrWhiteSpace(query.Discipline)) items = items.Where(e => e.Disciplines.Any(d => d.Slug == query.Discipline));
        if (!string.IsNullOrWhiteSpace(query.Tag)) items = items.Where(e => e.Tags.Any(t => t.Slug == query.Tag));
        if (query.Format is { } format) items = items.Where(e => e.Format == format);
        if (query.Type is { } type) items = items.Where(e => e.Type == type);
        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var term = query.Q.Trim();
            items = items.Where(e =>
                e.Title.Ka.Contains(term) || (e.Title.En != null && e.Title.En.Contains(term)) ||
                e.Summary.Ka.Contains(term) || (e.Summary.En != null && e.Summary.En.Contains(term)) ||
                e.Participants.Any(p => p.Person.FullName.Ka.Contains(term) || (p.Person.FullName.En != null && p.Person.FullName.En.Contains(term))));
        }

        items = query.Scope == EventScope.Upcoming
            ? items.OrderBy(e => e.StartsAtUtc)
            : items.OrderByDescending(e => e.StartsAtUtc);

        var page = await items.WithCardData().ToPagedAsync(query, ct);
        return page.Map(e => cards.Event(e, project));
    }

    /// <summary>Values that actually occur in published events, for the archive filters.</summary>
    public async Task<EventFiltersDto> FiltersAsync(string projectSlug, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var events = db.Events.AsNoTracking().Live(project.Id);

        var starts = await events.Select(e => e.StartsAtUtc).ToListAsync(ct);
        var years = starts.Select(s => TbilisiTime.ToLocal(s).Year).Distinct().OrderByDescending(y => y).ToList();

        var disciplines = await events.SelectMany(e => e.Disciplines).Distinct().ToListAsync(ct);
        var tags = await events.SelectMany(e => e.Tags).Distinct().ToListAsync(ct);

        return new EventFiltersDto(
            years,
            map.Terms(disciplines),
            map.Terms(tags),
            EnumLabels.All<EventFormat>(map.Language),
            EnumLabels.All<EventType>(map.Language));
    }

    public async Task<EventDetailDto> GetAsync(string projectSlug, string slug, CancellationToken ct)
    {
        var project = await projects.ResolveAsync(projectSlug, ct);
        var now = cards.UtcNow;

        var item = await db.Events.AsNoTracking()
            .Live(project.Id)
            .WithCardData()
            .Include(e => e.ShareImage)
            .Include(e => e.Gallery).ThenInclude(g => g.MediaAsset)
            .FirstOrDefaultAsync(e => e.Slug == slug, ct);

        if (item is null)
        {
            var targetId = await slugs.FindRedirectAsync(SluggedContentType.Event, project.Id, slug, ct);
            var moved = targetId == Guid.Empty
                ? null
                : await db.Events.AsNoTracking().Live(project.Id).Where(e => e.Id == targetId).Select(e => e.Slug).FirstOrDefaultAsync(ct);
            throw moved is null ? new NotFoundException("Event", slug) : (Exception)new SlugMovedException(moved);
        }

        var resources = await db.Resources.AsNoTracking()
            .Live(project.Id, now)
            .Where(r => r.EventId == item.Id)
            .OrderBy(r => r.Type).ThenBy(r => r.CreatedAtUtc)
            .WithCardData()
            .ToListAsync(ct);

        var news = await db.News.AsNoTracking()
            .Live(project.Id, now)
            .Where(n => n.RelatedEventId == item.Id)
            .OrderByDescending(n => n.PublishAtUtc)
            .Take(6)
            .WithCardData()
            .ToListAsync(ct);

        var title = map.Text(item.Title);
        var description = map.Text(item.Description);
        var path = PublicPaths.Event(project.Slug, item.Slug);
        var isCompleted = item.Status == EventStatus.Completed;
        var hasStatusNote = item.Status is EventStatus.Cancelled or EventStatus.Postponed;

        return new EventDetailDto(
            item.Slug,
            title,
            map.TextOrNull(item.Summary),
            description,
            item.Type,
            EnumLabels.Of(item.Type, map.Language),
            item.Status,
            EnumLabels.Of(item.Status, map.Language),
            hasStatusNote ? map.TextOrNull(item.StatusNote) : null,
            item.Format,
            EnumLabels.Of(item.Format, map.Language),
            TbilisiTime.ToLocal(item.StartsAtUtc),
            TbilisiTime.ToLocal(item.EndsAtUtc),
            TbilisiTime.ToLocal(item.OriginalStartsAtUtc),
            TbilisiTime.TimeZoneId,
            map.TextOrNull(item.Venue),
            item.Status == EventStatus.Cancelled ? null : item.StreamUrl,
            item.RegistrationUrl,
            TbilisiTime.ToLocal(item.RegistrationDeadlineUtc),
            ContentCards.IsRegistrationOpen(item, now),
            map.Media(item.CoverImage),
            cards.Participants(item, project),
            map.Terms(item.Disciplines),
            map.Terms(item.Tags),
            isCompleted ? map.TextOrNull(item.Recap) : null,
            isCompleted ? item.RecordingUrl : null,
            isCompleted ? VideoEmbed.ToEmbedUrl(item.RecordingUrl) : null,
            item.Gallery.OrderBy(g => g.SortOrder).Select(g => map.Media(g.MediaAsset)!).ToList(),
            resources.Select(r => cards.Resource(r, project)).ToList(),
            news.Select(n => cards.News(n, project)).ToList(),
            map.Seo(item, item.ShareImage, title, map.TextOrNull(item.Summary) ?? description, item.CoverImage, path),
            path);
    }
}
