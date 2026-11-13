using StemDialogue.Domain.Common;
using StemDialogue.Domain.Media;
using StemDialogue.Domain.People;
using StemDialogue.Domain.Resources;
using StemDialogue.Domain.Taxonomy;

namespace StemDialogue.Domain.Events;

public enum EventType
{
    StemSaturday = 0,
    Workshop = 1,
    Lecture = 2,
    Forum = 3,
    FinalEvent = 4,
    Other = 5,
}

public enum EventFormat
{
    Online = 0,
    InPerson = 1,
    Hybrid = 2,
}

/// <summary>Lifecycle from spec §6. Publication visibility is tracked separately.</summary>
public enum EventStatus
{
    Planned = 0,
    Postponed = 1,
    Completed = 2,
    Cancelled = 3,
}

public enum EventParticipantRole
{
    Speaker = 0,
    Researcher = 1,
    Mentor = 2,
    Moderator = 3,
    Guest = 4,
}

/// <summary>
/// A STEM Saturday or other project event. It is published first as an announcement and later,
/// at the same URL, completed with its summary, recording, photos and materials (spec §6).
/// </summary>
public class Event : AuditableEntity, IHasSlug, IHasSeo
{
    public Guid ProjectId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public EventType Type { get; set; } = EventType.StemSaturday;
    public LocalizedText Title { get; set; } = new();
    public LocalizedText Summary { get; set; } = new();

    /// <summary>Announcement text (sanitized HTML).</summary>
    public LocalizedText Description { get; set; } = new();

    public DateTime StartsAtUtc { get; set; }
    public DateTime? EndsAtUtc { get; set; }
    public EventFormat Format { get; set; } = EventFormat.Online;
    public LocalizedText Venue { get; set; } = new();

    /// <summary>Public livestream or meeting page, if the organizers choose to publish it.</summary>
    public string? StreamUrl { get; set; }
    public string? RegistrationUrl { get; set; }
    public DateTime? RegistrationDeadlineUtc { get; set; }

    public EventStatus Status { get; set; } = EventStatus.Planned;

    /// <summary>Public explanation of a cancellation or postponement.</summary>
    public LocalizedText StatusNote { get; set; } = new();
    public DateTime? OriginalStartsAtUtc { get; set; }

    /// <summary>Post-event summary (sanitized HTML).</summary>
    public LocalizedText Recap { get; set; } = new();
    public string? RecordingUrl { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    public PublicationStatus Publication { get; set; } = PublicationStatus.Draft;
    public DateTime? FirstPublishedAtUtc { get; set; }
    public bool IsFeatured { get; set; }

    public Guid? CoverImageId { get; set; }
    public MediaAsset? CoverImage { get; set; }

    public List<EventParticipant> Participants { get; set; } = [];
    public List<EventGalleryItem> Gallery { get; set; } = [];
    public List<Tag> Tags { get; set; } = [];
    public List<Discipline> Disciplines { get; set; } = [];
    public List<Resource> Resources { get; set; } = [];

    public LocalizedText SeoTitle { get; set; } = new();
    public LocalizedText SeoDescription { get; set; } = new();
    public Guid? ShareImageId { get; set; }
    public MediaAsset? ShareImage { get; set; }

    public bool IsUpcoming(DateTime nowUtc) =>
        Status is EventStatus.Planned or EventStatus.Postponed && (EndsAtUtc ?? StartsAtUtc) >= nowUtc;

    public void Complete(DateTime nowUtc)
    {
        if (Status == EventStatus.Cancelled)
            throw new DomainRuleException("A cancelled event cannot be completed. Restore it first.");
        if (StartsAtUtc > nowUtc)
            throw new DomainRuleException("An event can be completed only after it has started.");
        Status = EventStatus.Completed;
        CompletedAtUtc ??= nowUtc;
    }

    public void Cancel(string noteKa, string? noteEn)
    {
        if (Status == EventStatus.Completed)
            throw new DomainRuleException("A completed event cannot be cancelled.");
        Status = EventStatus.Cancelled;
        StatusNote.Set(noteKa, noteEn);
    }

    public void Postpone(DateTime newStartsAtUtc, DateTime? newEndsAtUtc, string noteKa, string? noteEn, DateTime nowUtc)
    {
        if (Status == EventStatus.Completed)
            throw new DomainRuleException("A completed event cannot be postponed.");
        if (newStartsAtUtc <= nowUtc)
            throw new DomainRuleException("The new date must be in the future.");
        if (newEndsAtUtc is not null && newEndsAtUtc <= newStartsAtUtc)
            throw new DomainRuleException("The end must be after the start.");

        OriginalStartsAtUtc ??= StartsAtUtc;
        StartsAtUtc = newStartsAtUtc;
        EndsAtUtc = newEndsAtUtc;
        Status = EventStatus.Postponed;
        StatusNote.Set(noteKa, noteEn);
    }

    public void Restore()
    {
        if (Status != EventStatus.Cancelled)
            throw new DomainRuleException("Only a cancelled event can be restored.");
        Status = EventStatus.Planned;
        StatusNote.Set(null, null);
    }
}

public class EventParticipant
{
    public Guid EventId { get; set; }
    public Guid PersonId { get; set; }
    public Person Person { get; set; } = null!;
    public EventParticipantRole Role { get; set; }
    public int SortOrder { get; set; }
}

public class EventGalleryItem
{
    public Guid EventId { get; set; }
    public Guid MediaAssetId { get; set; }
    public MediaAsset MediaAsset { get; set; } = null!;
    public int SortOrder { get; set; }
}
