using StemDialogue.Application.Abstractions;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.Media;
using StemDialogue.Domain.Taxonomy;

namespace StemDialogue.Application.Common;

public sealed record MediaDto(
    Guid Id,
    string Url,
    string? Alt,
    string? Caption,
    string? Credit,
    int? Width,
    int? Height,
    string ContentType,
    long SizeBytes,
    MediaKind Kind);

/// <summary>Page title, meta description and social preview for Angular SSR/prerendering (spec §11, §15.20).</summary>
public sealed record SeoDto(string Title, string Description, string? ImageUrl, string CanonicalPath);

public sealed record TermDto(string Slug, string Name);

public sealed record LabeledValue(string Value, string Label);

/// <summary>Public URL paths of the Angular routes (spec §3).</summary>
public static class PublicPaths
{
    public static string Project(string project) => $"/projects/{project}";
    public static string Page(string project, string key) =>
        key == Domain.Pages.PageKeys.Home ? Project(project) : $"{Project(project)}/{key}";
    public static string News(string project, string slug) => $"{Project(project)}/news/{slug}";
    public static string Event(string project, string slug) => $"{Project(project)}/stem-saturdays/{slug}";
    public static string Researcher(string project, string slug) => $"{Project(project)}/researchers/{slug}";
    public static string Mentor(string project, string slug) => $"{Project(project)}/mentors/{slug}";
    public static string TeamMember(string project, string slug) => $"{Project(project)}/team/{slug}";
    public static string Resource(string project, string slug) => $"{Project(project)}/resources/{slug}";
    public static string SchoolActivity(string project, string slug) => $"{Project(project)}/stem-ambassadors/{slug}";
}

/// <summary>Maps entities to public read models in the request language.</summary>
public sealed class PublicMapper(ILanguageContext language, IFileStorage storage)
{
    public string Language => language.Language;

    public string Text(LocalizedText text) => text.Resolve(language.Language);

    public string? TextOrNull(LocalizedText text) => text.ResolveOrNull(language.Language);

    public MediaDto? Media(MediaAsset? asset) => asset is null
        ? null
        : new MediaDto(
            asset.Id,
            storage.GetPublicUrl(asset.StoragePath),
            asset.AltText.ResolveOrNull(language.Language),
            asset.Caption.ResolveOrNull(language.Language),
            asset.Credit.ResolveOrNull(language.Language),
            asset.Width,
            asset.Height,
            asset.ContentType,
            asset.SizeBytes,
            asset.Kind);

    public IReadOnlyList<TermDto> Terms(IEnumerable<Tag> tags) =>
        tags.OrderBy(t => t.Slug).Select(t => new TermDto(t.Slug, Text(t.Name))).ToList();

    public IReadOnlyList<TermDto> Terms(IEnumerable<Discipline> disciplines) =>
        disciplines.OrderBy(d => d.DisplayOrder).ThenBy(d => d.Slug).Select(d => new TermDto(d.Slug, Text(d.Name))).ToList();

    /// <summary>Builds SEO metadata, falling back to the content title, summary and cover image.</summary>
    public SeoDto Seo(IHasSeo? overrides, MediaAsset? shareImage, string title, string? description, MediaAsset? fallbackImage, string path)
    {
        var seoTitle = overrides is null ? null : TextOrNull(overrides.SeoTitle);
        var seoDescription = overrides is null ? null : TextOrNull(overrides.SeoDescription);
        var image = shareImage ?? fallbackImage;
        return new SeoDto(
            seoTitle ?? title,
            seoDescription ?? TextUtils.Excerpt(description, 160),
            image is null ? null : storage.GetPublicUrl(image.StoragePath),
            path);
    }
}
