using FluentValidation;
using StemDialogue.Application.Common;
using StemDialogue.Application.Events;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.News;

namespace StemDialogue.Application.News;

// ---------- Public ----------

public sealed class NewsQuery : PageQuery
{
    public NewsCategory? Category { get; set; }
    public string? Tag { get; set; }
    public int? Year { get; set; }

    /// <summary>Free-text search in title and summary.</summary>
    public string? Q { get; set; }
}

public sealed record NewsCardDto(
    string Slug,
    string Title,
    string Summary,
    NewsCategory Category,
    string CategoryLabel,
    DateTimeOffset PublishedAt,
    MediaDto? Cover,
    IReadOnlyList<TermDto> Tags,
    bool IsFeatured,
    string Path);

public sealed record NewsDetailDto(
    string Slug,
    string Title,
    string Summary,
    string BodyHtml,
    NewsCategory Category,
    string CategoryLabel,
    DateTimeOffset PublishedAt,
    DateTimeOffset UpdatedAt,
    MediaDto? Cover,
    IReadOnlyList<MediaDto> Gallery,
    IReadOnlyList<TermDto> Tags,
    EventCardDto? RelatedEvent,
    IReadOnlyList<NewsCardDto> Related,
    SeoDto Seo,
    string Path);

public sealed record CategoryCountDto(NewsCategory Value, string Label, int Count);

// ---------- Admin ----------

public sealed class NewsAdminQuery : PageQuery
{
    public PublicationStatus? Status { get; set; }
    public NewsCategory? Category { get; set; }
    public string? Search { get; set; }
}

public sealed record NewsUpsertRequest(
    LocalizedTextDto Title,
    string? Slug,
    LocalizedTextDto? Summary,
    LocalizedTextDto? Body,
    NewsCategory Category,
    Guid? CoverImageId,
    IReadOnlyList<Guid>? GalleryImageIds,
    IReadOnlyList<Guid>? TagIds,
    Guid? RelatedEventId,
    bool IsFeatured,
    LocalizedTextDto? SeoTitle,
    LocalizedTextDto? SeoDescription,
    Guid? ShareImageId,
    Guid? ConcurrencyStamp);

/// <summary>Publishes now, or at <see cref="PublishAtUtc"/> (future = scheduled, past = back-dated import).</summary>
public sealed record PublishRequest(DateTime? PublishAtUtc);

public sealed record NewsAdminListItem(
    Guid Id,
    string Slug,
    string Title,
    NewsCategory Category,
    PublicationStatus Status,
    bool IsLive,
    bool IsScheduled,
    DateTime? PublishAtUtc,
    bool IsFeatured,
    DateTime UpdatedAtUtc,
    string? UpdatedBy,
    string PublicPath);

public sealed record NewsAdminDto(
    Guid Id,
    string Slug,
    LocalizedTextDto Title,
    LocalizedTextDto Summary,
    LocalizedTextDto Body,
    NewsCategory Category,
    PublicationStatus Status,
    DateTime? PublishAtUtc,
    DateTime? FirstPublishedAtUtc,
    bool IsFeatured,
    AdminMediaRef? CoverImage,
    IReadOnlyList<AdminMediaRef> Gallery,
    IReadOnlyList<Guid> TagIds,
    Guid? RelatedEventId,
    LocalizedTextDto SeoTitle,
    LocalizedTextDto SeoDescription,
    AdminMediaRef? ShareImage,
    string PublicPath,
    AuditInfo Audit);

public sealed class NewsUpsertValidator : AbstractValidator<NewsUpsertRequest>
{
    public NewsUpsertValidator()
    {
        RuleFor(x => x.Title).RequiredText(Limits.Title);
        RuleFor(x => x.Slug).OptionalSlug();
        RuleFor(x => x.Summary).OptionalText(Limits.Summary);
        RuleFor(x => x.Body).OptionalText(Limits.Html);
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.GalleryImageIds).Must(x => x is null || x.Count <= 60).WithMessage("A gallery can hold at most 60 images.");
        RuleFor(x => x.TagIds).Must(x => x is null || x.Count <= 20).WithMessage("At most 20 tags.");
        RuleFor(x => x.SeoTitle).OptionalText(Limits.SeoTitle);
        RuleFor(x => x.SeoDescription).OptionalText(Limits.SeoDescription);
    }
}
