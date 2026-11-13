using System.Security.Cryptography;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StemDialogue.Application.Abstractions;
using StemDialogue.Application.Common;
using StemDialogue.Domain.Media;

namespace StemDialogue.Application.Media;

public sealed record MediaUpload(
    Stream Content,
    string FileName,
    long Length,
    LocalizedTextDto? AltText,
    LocalizedTextDto? Caption,
    LocalizedTextDto? Credit);

public sealed record MediaMetadataRequest(LocalizedTextDto? AltText, LocalizedTextDto? Caption, LocalizedTextDto? Credit, Guid ConcurrencyStamp);

public sealed class MediaAdminQuery : PageQuery
{
    public MediaKind? Kind { get; set; }
    public string? Search { get; set; }
}

public sealed record MediaAdminDto(
    Guid Id,
    string Url,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    MediaKind Kind,
    int? Width,
    int? Height,
    LocalizedTextDto AltText,
    LocalizedTextDto Caption,
    LocalizedTextDto Credit,
    string Sha256,
    bool MissingAltText,
    AuditInfo Audit);

public sealed record MediaUsageDto(string ContentType, Guid Id, string Title);

public sealed class MediaMetadataValidator : AbstractValidator<MediaMetadataRequest>
{
    public MediaMetadataValidator()
    {
        RuleFor(x => x.AltText).OptionalText(250);
        RuleFor(x => x.Caption).OptionalText(500);
        RuleFor(x => x.Credit).OptionalText(150);
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

/// <summary>Uploads with type/size checks, metadata (alt text) and safe deletion (spec §10, §11).</summary>
public sealed class MediaService(
    IAppDbContext db,
    IFileStorage storage,
    IOptions<MediaOptions> options,
    TimeProvider clock)
{
    public async Task<MediaAdminDto> UploadAsync(MediaUpload upload, CancellationToken ct)
    {
        var fileName = Path.GetFileName(upload.FileName ?? string.Empty);
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > 255)
            throw ContentReferences.Invalid("File", "A file name of at most 255 characters is required.");

        var limit = Math.Max(options.Value.MaxImageBytes, options.Value.MaxDocumentBytes);
        if (upload.Length > limit)
            throw ContentReferences.Invalid("File", $"The file is too large. The maximum is {limit / (1024 * 1024)} MB.");

        using var buffer = new MemoryStream();
        await upload.Content.CopyToAsync(buffer, ct);
        var bytes = buffer.ToArray();

        var (file, error) = MediaInspector.Inspect(fileName, bytes, bytes.LongLength, options.Value);
        if (file is null) throw ContentReferences.Invalid("File", error ?? "The file was rejected.");

        var now = clock.GetUtcNow().UtcDateTime;
        var id = Guid.NewGuid();
        var storagePath = $"{now:yyyy}/{now:MM}/{id:N}{file.Extension}";

        var asset = new MediaAsset
        {
            Id = id,
            StoragePath = storagePath,
            OriginalFileName = fileName,
            ContentType = file.ContentType,
            SizeBytes = bytes.LongLength,
            Kind = file.Kind,
            Width = file.Width,
            Height = file.Height,
            Sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),
        };
        asset.AltText.Apply(upload.AltText);
        asset.Caption.Apply(upload.Caption);
        asset.Credit.Apply(upload.Credit);

        buffer.Position = 0;
        await storage.SaveAsync(buffer, storagePath, ct);
        try
        {
            db.Media.Add(asset);
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            await storage.DeleteAsync(storagePath, CancellationToken.None);
            throw;
        }

        return ToDto(asset);
    }

    public async Task<PagedResult<MediaAdminDto>> ListAsync(MediaAdminQuery query, CancellationToken ct)
    {
        var items = db.Media.AsNoTracking();
        if (query.Kind is { } kind) items = items.Where(m => m.Kind == kind);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            items = items.Where(m => m.OriginalFileName.Contains(term) || m.AltText.Ka.Contains(term) || m.Caption.Ka.Contains(term));
        }

        var page = await items.OrderByDescending(m => m.CreatedAtUtc).ToPagedAsync(query, ct);
        return page.Map(ToDto);
    }

    public async Task<MediaAdminDto> GetAsync(Guid id, CancellationToken ct) =>
        ToDto(await db.Media.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, ct) ?? throw new NotFoundException("Media", id));

    public async Task<MediaAdminDto> UpdateMetadataAsync(Guid id, MediaMetadataRequest request, CancellationToken ct)
    {
        var asset = await db.Media.FirstOrDefaultAsync(m => m.Id == id, ct) ?? throw new NotFoundException("Media", id);
        db.EnsureStamp(asset, request.ConcurrencyStamp);
        asset.AltText.Apply(request.AltText);
        asset.Caption.Apply(request.Caption);
        asset.Credit.Apply(request.Credit);
        await db.SaveChangesAsync(ct);
        return ToDto(asset);
    }

    /// <summary>Lists every place a file is used, so editors know why it cannot be deleted.</summary>
    public async Task<IReadOnlyList<MediaUsageDto>> UsagesAsync(Guid id, CancellationToken ct)
    {
        var usages = new List<MediaUsageDto>();

        usages.AddRange(await db.News.AsNoTracking()
            .Where(n => n.CoverImageId == id || n.ShareImageId == id || n.Gallery.Any(g => g.MediaAssetId == id))
            .Select(n => new MediaUsageDto("News", n.Id, n.Title.Ka)).ToListAsync(ct));
        usages.AddRange(await db.Events.AsNoTracking()
            .Where(e => e.CoverImageId == id || e.ShareImageId == id || e.Gallery.Any(g => g.MediaAssetId == id))
            .Select(e => new MediaUsageDto("Event", e.Id, e.Title.Ka)).ToListAsync(ct));
        usages.AddRange(await db.People.AsNoTracking()
            .Where(p => p.PhotoId == id || p.ShareImageId == id)
            .Select(p => new MediaUsageDto("Person", p.Id, p.FullName.Ka)).ToListAsync(ct));
        usages.AddRange(await db.Resources.AsNoTracking()
            .Where(r => r.FileId == id || r.ThumbnailId == id)
            .Select(r => new MediaUsageDto("Resource", r.Id, r.Title.Ka)).ToListAsync(ct));
        usages.AddRange(await db.SchoolActivities.AsNoTracking()
            .Where(a => a.CoverImageId == id || a.Gallery.Any(g => g.MediaAssetId == id))
            .Select(a => new MediaUsageDto("SchoolActivity", a.Id, a.Title.Ka)).ToListAsync(ct));
        usages.AddRange(await db.Pages.AsNoTracking()
            .Where(p => p.HeroImageId == id || p.ShareImageId == id)
            .Select(p => new MediaUsageDto("Page", p.Id, p.Title.Ka)).ToListAsync(ct));
        usages.AddRange(await db.Organizations.AsNoTracking()
            .Where(o => o.LogoId == id || o.LogoOnDarkId == id)
            .Select(o => new MediaUsageDto("Organization", o.Id, o.Name.Ka)).ToListAsync(ct));
        usages.AddRange(await db.Projects.AsNoTracking()
            .Where(p => p.LogoId == id || p.ShareImageId == id)
            .Select(p => new MediaUsageDto("Project", p.Id, p.Name.Ka)).ToListAsync(ct));

        return usages;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var asset = await db.Media.FirstOrDefaultAsync(m => m.Id == id, ct) ?? throw new NotFoundException("Media", id);
        var usages = await UsagesAsync(id, ct);
        if (usages.Count > 0)
        {
            var where = string.Join("; ", usages.Take(5).Select(u => $"{u.ContentType}: {u.Title}"));
            throw new ConflictException($"The file is still used ({where}). Remove it from that content first.");
        }

        db.Media.Remove(asset);
        await db.SaveChangesAsync(ct);
        await storage.DeleteAsync(asset.StoragePath, ct);
    }

    private MediaAdminDto ToDto(MediaAsset m) => new(
        m.Id,
        storage.GetPublicUrl(m.StoragePath),
        m.OriginalFileName,
        m.ContentType,
        m.SizeBytes,
        m.Kind,
        m.Width,
        m.Height,
        m.AltText.ToDto(),
        m.Caption.ToDto(),
        m.Credit.ToDto(),
        m.Sha256,
        m.Kind == MediaKind.Image && string.IsNullOrWhiteSpace(m.AltText.Ka),
        AuditInfo.Of(m));
}
