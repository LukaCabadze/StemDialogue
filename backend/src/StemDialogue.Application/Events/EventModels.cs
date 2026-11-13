using FluentValidation;
using StemDialogue.Application.Common;
using StemDialogue.Application.News;
using StemDialogue.Application.People;
using StemDialogue.Application.Resources;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.Events;

namespace StemDialogue.Application.Events;

// ---------- Public ----------

public enum EventScope
{
    Upcoming = 0,
    Past = 1,
    All = 2,
}

public sealed class EventQuery : PageQuery
{
    public EventScope Scope { get; set; } = EventScope.Upcoming;
    public int? Year { get; set; }
    public int? Month { get; set; }
    public string? Discipline { get; set; }
    public string? Tag { get; set; }
    public EventFormat? Format { get; set; }
    public EventType? Type { get; set; }
    public string? Q { get; set; }
}

public sealed record EventCardDto(
    string Slug,
    string Title,
    string? Summary,
    EventType Type,
    string TypeLabel,
    EventStatus Status,
    string StatusLabel,
    EventFormat Format,
    string FormatLabel,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt,
    string? Venue,
    MediaDto? Cover,
    IReadOnlyList<PersonRefDto> Speakers,
    IReadOnlyList<TermDto> Disciplines,
    IReadOnlyList<TermDto> Tags,
    bool IsUpcoming,
    bool HasRecording,
    bool HasMaterials,
    string? RegistrationUrl,
    string Path);

/// <summary>
/// One URL for the whole lifecycle: announcement fields first, then the archive fields
/// (<see cref="RecapHtml"/>, recording, gallery, resources) once the event is completed (spec §6).
/// </summary>
public sealed record EventDetailDto(
    string Slug,
    string Title,
    string? Summary,
    string DescriptionHtml,
    EventType Type,
    string TypeLabel,
    EventStatus Status,
    string StatusLabel,
    string? StatusNote,
    EventFormat Format,
    string FormatLabel,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt,
    DateTimeOffset? OriginalStartsAt,
    string TimeZone,
    string? Venue,
    string? StreamUrl,
    string? RegistrationUrl,
    DateTimeOffset? RegistrationDeadline,
    bool IsRegistrationOpen,
    MediaDto? Cover,
    IReadOnlyList<PersonRefDto> Participants,
    IReadOnlyList<TermDto> Disciplines,
    IReadOnlyList<TermDto> Tags,
    string? RecapHtml,
    string? RecordingUrl,
    string? RecordingEmbedUrl,
    IReadOnlyList<MediaDto> Gallery,
    IReadOnlyList<ResourceCardDto> Resources,
    IReadOnlyList<NewsCardDto> RelatedNews,
    SeoDto Seo,
    string Path);

public sealed record EventFiltersDto(
    IReadOnlyList<int> Years,
    IReadOnlyList<TermDto> Disciplines,
    IReadOnlyList<TermDto> Tags,
    IReadOnlyList<LabeledValue> Formats,
    IReadOnlyList<LabeledValue> Types);

// ---------- Admin ----------

public sealed class EventAdminQuery : PageQuery
{
    public EventScope Scope { get; set; } = EventScope.All;
    public EventStatus? Status { get; set; }
    public PublicationStatus? Publication { get; set; }
    public string? Search { get; set; }
}

public sealed record EventParticipantInput(Guid PersonId, EventParticipantRole Role);

public sealed record EventUpsertRequest(
    EventType Type,
    LocalizedTextDto Title,
    string? Slug,
    LocalizedTextDto? Summary,
    LocalizedTextDto? Description,
    DateTime StartsAtUtc,
    DateTime? EndsAtUtc,
    EventFormat Format,
    LocalizedTextDto? Venue,
    string? StreamUrl,
    string? RegistrationUrl,
    DateTime? RegistrationDeadlineUtc,
    Guid? CoverImageId,
    IReadOnlyList<EventParticipantInput>? Participants,
    IReadOnlyList<Guid>? TagIds,
    IReadOnlyList<Guid>? DisciplineIds,
    bool IsFeatured,
    LocalizedTextDto? Recap,
    string? RecordingUrl,
    IReadOnlyList<Guid>? GalleryImageIds,
    LocalizedTextDto? SeoTitle,
    LocalizedTextDto? SeoDescription,
    Guid? ShareImageId,
    Guid? ConcurrencyStamp);

/// <summary>Marks the event as held; optionally adds the summary and recording in the same step.</summary>
public sealed record CompleteEventRequest(LocalizedTextDto? Recap, string? RecordingUrl);

public sealed record CancelEventRequest(LocalizedTextDto Note);

public sealed record PostponeEventRequest(DateTime NewStartsAtUtc, DateTime? NewEndsAtUtc, LocalizedTextDto Note);

public sealed record EventAdminListItem(
    Guid Id,
    string Slug,
    string Title,
    EventType Type,
    EventStatus Status,
    PublicationStatus Publication,
    DateTime StartsAtUtc,
    bool IsFeatured,
    bool AwaitingCompletion,
    int ParticipantCount,
    DateTime UpdatedAtUtc,
    string? UpdatedBy,
    string PublicPath);

public sealed record EventParticipantAdminDto(Guid PersonId, string FullName, EventParticipantRole Role, int SortOrder);

public sealed record EventAdminDto(
    Guid Id,
    string Slug,
    EventType Type,
    LocalizedTextDto Title,
    LocalizedTextDto Summary,
    LocalizedTextDto Description,
    DateTime StartsAtUtc,
    DateTime? EndsAtUtc,
    DateTime? OriginalStartsAtUtc,
    EventFormat Format,
    LocalizedTextDto Venue,
    string? StreamUrl,
    string? RegistrationUrl,
    DateTime? RegistrationDeadlineUtc,
    EventStatus Status,
    LocalizedTextDto StatusNote,
    PublicationStatus Publication,
    DateTime? FirstPublishedAtUtc,
    bool IsFeatured,
    AdminMediaRef? CoverImage,
    IReadOnlyList<EventParticipantAdminDto> Participants,
    IReadOnlyList<Guid> TagIds,
    IReadOnlyList<Guid> DisciplineIds,
    LocalizedTextDto Recap,
    string? RecordingUrl,
    DateTime? CompletedAtUtc,
    IReadOnlyList<AdminMediaRef> Gallery,
    LocalizedTextDto SeoTitle,
    LocalizedTextDto SeoDescription,
    AdminMediaRef? ShareImage,
    string PublicPath,
    AuditInfo Audit);

public sealed class EventUpsertValidator : AbstractValidator<EventUpsertRequest>
{
    public EventUpsertValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Title).RequiredText(Limits.Title);
        RuleFor(x => x.Slug).OptionalSlug();
        RuleFor(x => x.Summary).OptionalText(Limits.Summary);
        RuleFor(x => x.Description).OptionalText(Limits.Html);
        RuleFor(x => x.StartsAtUtc).NotEmpty();
        RuleFor(x => x.EndsAtUtc).GreaterThan(x => x.StartsAtUtc).When(x => x.EndsAtUtc is not null)
            .WithMessage("The end must be after the start.");
        RuleFor(x => x.EndsAtUtc).Must((x, end) => end is null || end.Value - x.StartsAtUtc <= TimeSpan.FromDays(7))
            .WithMessage("An event can last at most 7 days.");
        RuleFor(x => x.Format).IsInEnum();
        RuleFor(x => x.Venue).OptionalText(Limits.Short);
        RuleFor(x => x.Venue).Must(v => v is not null && !string.IsNullOrWhiteSpace(v.Ka))
            .When(x => x.Format != EventFormat.Online)
            .WithMessage("A venue is required for in-person and hybrid events.");
        RuleFor(x => x.StreamUrl).HttpsUrl();
        RuleFor(x => x.RegistrationUrl).HttpsUrl();
        RuleFor(x => x.RecordingUrl).HttpsUrl();
        RuleFor(x => x.Recap).OptionalText(Limits.Html);
        RuleFor(x => x.Participants)
            .Must(p => p is null || p.Select(x => x.PersonId).Distinct().Count() == p.Count)
            .WithMessage("A person can be added to an event only once.");
        RuleForEach(x => x.Participants).ChildRules(p => p.RuleFor(x => x.Role).IsInEnum());
        RuleFor(x => x.GalleryImageIds).Must(x => x is null || x.Count <= 100).WithMessage("A gallery can hold at most 100 images.");
        RuleFor(x => x.SeoTitle).OptionalText(Limits.SeoTitle);
        RuleFor(x => x.SeoDescription).OptionalText(Limits.SeoDescription);
    }
}

public sealed class CompleteEventValidator : AbstractValidator<CompleteEventRequest>
{
    public CompleteEventValidator()
    {
        RuleFor(x => x.Recap).OptionalText(Limits.Html);
        RuleFor(x => x.RecordingUrl).HttpsUrl();
    }
}

public sealed class CancelEventValidator : AbstractValidator<CancelEventRequest>
{
    public CancelEventValidator() => RuleFor(x => x.Note).RequiredText(Limits.Summary);
}

public sealed class PostponeEventValidator : AbstractValidator<PostponeEventRequest>
{
    public PostponeEventValidator()
    {
        RuleFor(x => x.NewStartsAtUtc).NotEmpty();
        RuleFor(x => x.NewEndsAtUtc).GreaterThan(x => x.NewStartsAtUtc).When(x => x.NewEndsAtUtc is not null);
        RuleFor(x => x.Note).RequiredText(Limits.Summary);
    }
}
