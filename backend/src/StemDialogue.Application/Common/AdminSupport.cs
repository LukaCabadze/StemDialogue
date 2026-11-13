using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using StemDialogue.Application.Abstractions;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.Media;
using StemDialogue.Domain.Taxonomy;

namespace StemDialogue.Application.Common;

/// <summary>Media reference returned to the admin panel for previews.</summary>
public sealed record AdminMediaRef(Guid Id, string Url, string OriginalFileName, string? AltKa, int? Width, int? Height, MediaKind Kind);

/// <summary>Who changed an item and when, plus the stamp required for the next update.</summary>
public sealed record AuditInfo(DateTime CreatedAtUtc, string? CreatedBy, DateTime UpdatedAtUtc, string? UpdatedBy, Guid ConcurrencyStamp)
{
    public static AuditInfo Of(AuditableEntity e) => new(e.CreatedAtUtc, e.CreatedBy, e.UpdatedAtUtc, e.UpdatedBy, e.ConcurrencyStamp);
}

public static class AdminMappings
{
    public static AdminMediaRef? ToRef(this MediaAsset? asset, IFileStorage storage) => asset is null
        ? null
        : new AdminMediaRef(asset.Id, storage.GetPublicUrl(asset.StoragePath), asset.OriginalFileName, TextUtils.NullIfBlank(asset.AltText.Ka), asset.Width, asset.Height, asset.Kind);
}

public static class ConcurrencyExtensions
{
    /// <summary>Rejects an update made from a stale copy and arms EF's concurrency check with the client's stamp.</summary>
    public static void EnsureStamp(this IAppDbContext db, AuditableEntity entity, Guid clientStamp)
    {
        if (entity.ConcurrencyStamp != clientStamp)
            throw new ConflictException("This item was changed by someone else. Reload it and apply your edits again.");
        db.UseClientConcurrencyStamp(entity, clientStamp);
    }
}

/// <summary>Validates and loads entities referenced by id from admin payloads.</summary>
public sealed class ContentReferences(IAppDbContext db)
{
    public async Task<List<Tag>> TagsAsync(IReadOnlyCollection<Guid>? ids, CancellationToken ct)
    {
        if (ids is null || ids.Count == 0) return [];
        var distinct = ids.Distinct().ToList();
        var tags = await db.Tags.Where(t => distinct.Contains(t.Id)).ToListAsync(ct);
        if (tags.Count != distinct.Count) throw Invalid("TagIds", "One or more tags do not exist.");
        return tags;
    }

    public async Task<List<Discipline>> DisciplinesAsync(IReadOnlyCollection<Guid>? ids, CancellationToken ct)
    {
        if (ids is null || ids.Count == 0) return [];
        var distinct = ids.Distinct().ToList();
        var items = await db.Disciplines.Where(t => distinct.Contains(t.Id)).ToListAsync(ct);
        if (items.Count != distinct.Count) throw Invalid("DisciplineIds", "One or more disciplines do not exist.");
        return items;
    }

    /// <summary>Ensures each non-null id refers to an uploaded image.</summary>
    public async Task EnsureImageAsync(Guid? id, string field, CancellationToken ct)
    {
        if (id is null) return;
        var kind = await db.Media.Where(m => m.Id == id).Select(m => (MediaKind?)m.Kind).FirstOrDefaultAsync(ct);
        if (kind is null) throw Invalid(field, "The selected file does not exist.");
        if (kind != MediaKind.Image) throw Invalid(field, "The selected file is not an image.");
    }

    public async Task EnsureFileAsync(Guid? id, string field, CancellationToken ct)
    {
        if (id is null) return;
        if (!await db.Media.AnyAsync(m => m.Id == id, ct)) throw Invalid(field, "The selected file does not exist.");
    }

    /// <summary>Loads gallery images in the requested order.</summary>
    public async Task<IReadOnlyList<Guid>> GalleryAsync(IReadOnlyList<Guid>? ids, string field, CancellationToken ct)
    {
        if (ids is null || ids.Count == 0) return [];
        var ordered = ids.Distinct().ToList();
        var images = await db.Media.Where(m => ordered.Contains(m.Id) && m.Kind == MediaKind.Image).Select(m => m.Id).ToListAsync(ct);
        if (images.Count != ordered.Count) throw Invalid(field, "Every gallery item must be an uploaded image.");
        return ordered;
    }

    public static ValidationException Invalid(string field, string message) =>
        new([new ValidationFailure(field, message)]);
}

/// <summary>Assigns unique slugs and keeps old public URLs alive through redirects.</summary>
public sealed class SlugManager(IAppDbContext db, TimeProvider clock)
{
    /// <summary>
    /// Returns the slug to store. When an already-public item gets a new slug, the old one is kept as a redirect.
    /// </summary>
    public async Task<string> AssignAsync(
        SluggedContentType type,
        Guid? projectId,
        Guid entityId,
        string? currentSlug,
        string? requested,
        string fallbackSource,
        bool wasPublic,
        Func<string, Task<bool>> isTakenByOther,
        CancellationToken ct)
    {
        var candidate = Slugifier.From(string.IsNullOrWhiteSpace(requested) ? fallbackSource : requested);
        if (candidate == currentSlug) return candidate;

        var slug = await Slugifier.EnsureUniqueAsync(candidate, isTakenByOther);
        if (slug == currentSlug) return slug;

        // A reclaimed slug must stop redirecting elsewhere.
        var stale = await db.SlugRedirects
            .Where(r => r.ContentType == type && r.ProjectId == projectId && r.OldSlug == slug)
            .ToListAsync(ct);
        db.SlugRedirects.RemoveRange(stale);

        if (wasPublic && !string.IsNullOrEmpty(currentSlug))
        {
            db.SlugRedirects.Add(new SlugRedirect
            {
                ContentType = type,
                ProjectId = projectId,
                OldSlug = currentSlug,
                TargetId = entityId,
                CreatedAtUtc = clock.GetUtcNow().UtcDateTime,
            });
        }

        return slug;
    }

    /// <summary>Finds the item an old slug now points to.</summary>
    public Task<Guid> FindRedirectAsync(SluggedContentType type, Guid? projectId, string slug, CancellationToken ct) =>
        db.SlugRedirects
            .Where(r => r.ContentType == type && r.ProjectId == projectId && r.OldSlug == slug)
            .Select(r => r.TargetId)
            .FirstOrDefaultAsync(ct);
}

public static class CollectionSync
{
    /// <summary>Makes <paramref name="current"/> contain exactly <paramref name="desired"/> (matched by id).</summary>
    public static void SyncById<T>(this List<T> current, IReadOnlyCollection<T> desired) where T : Entity
    {
        var desiredIds = desired.Select(d => d.Id).ToHashSet();
        current.RemoveAll(c => !desiredIds.Contains(c.Id));
        var currentIds = current.Select(c => c.Id).ToHashSet();
        current.AddRange(desired.Where(d => !currentIds.Contains(d.Id)));
    }

    /// <summary>
    /// Synchronizes an ordered join collection keyed by <paramref name="keyOf"/> without
    /// deleting and re-inserting rows that are kept.
    /// </summary>
    public static void SyncOrdered<T>(this List<T> current, IReadOnlyList<Guid> desiredKeys, Func<T, Guid> keyOf, Func<Guid, T> create, Action<T, int> setOrder)
    {
        var keys = desiredKeys.ToList();
        current.RemoveAll(item => !keys.Contains(keyOf(item)));
        for (var i = 0; i < keys.Count; i++)
        {
            var existing = current.FirstOrDefault(item => keyOf(item) == keys[i]);
            if (existing is null)
            {
                existing = create(keys[i]);
                current.Add(existing);
            }

            setOrder(existing, i);
        }
    }
}
