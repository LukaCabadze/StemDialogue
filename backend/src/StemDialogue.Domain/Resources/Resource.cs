using StemDialogue.Domain.Common;
using StemDialogue.Domain.Events;
using StemDialogue.Domain.Media;
using StemDialogue.Domain.People;
using StemDialogue.Domain.Taxonomy;

namespace StemDialogue.Domain.Resources;

public enum ResourceType
{
    Document = 0,
    Presentation = 1,
    Video = 2,
    Photo = 3,
    Link = 4,
    Other = 5,
}

/// <summary>
/// A learning material: an uploaded file or a stable external link / video embed (spec §9).
/// </summary>
public class Resource : AuditableEntity, IHasSlug
{
    public Guid ProjectId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public LocalizedText Title { get; set; } = new();
    public LocalizedText Description { get; set; } = new();
    public ResourceType Type { get; set; }

    public Guid? FileId { get; set; }
    public MediaAsset? File { get; set; }
    public string? ExternalUrl { get; set; }

    /// <summary>Linked author profile, when the author has one.</summary>
    public Guid? AuthorId { get; set; }
    public Person? Author { get; set; }

    /// <summary>Free-text author or speaker, for people without a profile.</summary>
    public LocalizedText AuthorName { get; set; } = new();

    public Guid? EventId { get; set; }
    public Event? Event { get; set; }

    public Guid? ThumbnailId { get; set; }
    public MediaAsset? Thumbnail { get; set; }

    public PublicationStatus Status { get; set; } = PublicationStatus.Draft;
    public DateTime? PublishedAtUtc { get; set; }
    public bool IsFeatured { get; set; }

    public List<Tag> Tags { get; set; } = [];
    public List<Discipline> Disciplines { get; set; } = [];
}
