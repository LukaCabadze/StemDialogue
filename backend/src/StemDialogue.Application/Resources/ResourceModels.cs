using FluentValidation;
using StemDialogue.Application.Common;
using StemDialogue.Application.People;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.Resources;

namespace StemDialogue.Application.Resources;

// ---------- Public ----------

public sealed class ResourceQuery : PageQuery
{
    public ResourceType? Type { get; set; }

    /// <summary>Event slug.</summary>
    public string? Event { get; set; }
    public string? Discipline { get; set; }
    public string? Tag { get; set; }
    public string? Q { get; set; }
}

public sealed record EventRefDto(string Slug, string Title, DateTimeOffset StartsAt, string Path);

public sealed record ResourceCardDto(
    string Slug,
    string Title,
    string? Description,
    ResourceType Type,
    string TypeLabel,
    string Url,
    string? EmbedUrl,
    MediaDto? File,
    MediaDto? Thumbnail,
    string? AuthorName,
    PersonRefDto? Author,
    EventRefDto? Event,
    DateTimeOffset? PublishedAt,
    IReadOnlyList<TermDto> Tags,
    string Path);

public sealed record ResourceDetailDto(
    ResourceCardDto Resource,
    IReadOnlyList<TermDto> Disciplines,
    IReadOnlyList<ResourceCardDto> Related,
    SeoDto Seo);

// ---------- Admin ----------

public sealed class ResourceAdminQuery : PageQuery
{
    public ResourceType? Type { get; set; }
    public PublicationStatus? Status { get; set; }
    public Guid? EventId { get; set; }
    public string? Search { get; set; }
}

public sealed record ResourceUpsertRequest(
    LocalizedTextDto Title,
    string? Slug,
    LocalizedTextDto? Description,
    ResourceType Type,
    Guid? FileId,
    string? ExternalUrl,
    Guid? AuthorId,
    LocalizedTextDto? AuthorName,
    Guid? EventId,
    Guid? ThumbnailId,
    PublicationStatus Status,
    DateTime? PublishedAtUtc,
    bool IsFeatured,
    IReadOnlyList<Guid>? TagIds,
    IReadOnlyList<Guid>? DisciplineIds,
    Guid? ConcurrencyStamp);

public sealed record ResourceAdminListItem(
    Guid Id,
    string Slug,
    string Title,
    ResourceType Type,
    PublicationStatus Status,
    bool IsFeatured,
    string? EventTitle,
    DateTime? PublishedAtUtc,
    DateTime UpdatedAtUtc,
    string? UpdatedBy);

public sealed record ResourceAdminDto(
    Guid Id,
    string Slug,
    LocalizedTextDto Title,
    LocalizedTextDto Description,
    ResourceType Type,
    AdminMediaRef? File,
    string? ExternalUrl,
    Guid? AuthorId,
    LocalizedTextDto AuthorName,
    Guid? EventId,
    AdminMediaRef? Thumbnail,
    PublicationStatus Status,
    DateTime? PublishedAtUtc,
    bool IsFeatured,
    IReadOnlyList<Guid> TagIds,
    IReadOnlyList<Guid> DisciplineIds,
    string PublicPath,
    AuditInfo Audit);

public sealed class ResourceUpsertValidator : AbstractValidator<ResourceUpsertRequest>
{
    public ResourceUpsertValidator()
    {
        RuleFor(x => x.Title).RequiredText(Limits.Title);
        RuleFor(x => x.Slug).OptionalSlug();
        RuleFor(x => x.Description).OptionalText(Limits.Summary * 2);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.ExternalUrl).HttpsUrl();
        RuleFor(x => x)
            .Must(x => (x.FileId is null) != string.IsNullOrWhiteSpace(x.ExternalUrl))
            .OverridePropertyName("FileId")
            .WithMessage("Provide either an uploaded file or an external link, not both.");
        RuleFor(x => x.ExternalUrl)
            .NotEmpty()
            .When(x => x.Type is ResourceType.Video or ResourceType.Link)
            .WithMessage("Videos and links must use an external URL; large media belongs on a video platform.");
        RuleFor(x => x.AuthorName).OptionalText(Limits.Name);
    }
}
