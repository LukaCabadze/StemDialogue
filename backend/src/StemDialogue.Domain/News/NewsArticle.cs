using StemDialogue.Domain.Common;
using StemDialogue.Domain.Events;
using StemDialogue.Domain.Media;
using StemDialogue.Domain.Taxonomy;

namespace StemDialogue.Domain.News;

/// <summary>News categories from spec §5.</summary>
public enum NewsCategory
{
    Announcement = 0,
    EventAnnouncement = 1,
    EventSummary = 2,
    Opportunity = 3,
    Achievement = 4,
    General = 5,
}

public class NewsArticle : AuditableEntity, IHasSlug, IHasSeo
{
    public Guid ProjectId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public LocalizedText Title { get; set; } = new();
    public LocalizedText Summary { get; set; } = new();

    /// <summary>Sanitized HTML.</summary>
    public LocalizedText Body { get; set; } = new();
    public NewsCategory Category { get; set; } = NewsCategory.General;
    public PublicationStatus Status { get; set; } = PublicationStatus.Draft;

    /// <summary>Publication moment. A future value schedules the article.</summary>
    public DateTime? PublishAtUtc { get; set; }

    /// <summary>Set once, the first time the article goes live. Used to protect the public URL.</summary>
    public DateTime? FirstPublishedAtUtc { get; set; }
    public bool IsFeatured { get; set; }

    public Guid? CoverImageId { get; set; }
    public MediaAsset? CoverImage { get; set; }

    public Guid? RelatedEventId { get; set; }
    public Event? RelatedEvent { get; set; }

    public List<Tag> Tags { get; set; } = [];
    public List<NewsGalleryItem> Gallery { get; set; } = [];

    public LocalizedText SeoTitle { get; set; } = new();
    public LocalizedText SeoDescription { get; set; } = new();
    public Guid? ShareImageId { get; set; }
    public MediaAsset? ShareImage { get; set; }

    public bool IsLive(DateTime nowUtc) =>
        Status == PublicationStatus.Published && PublishAtUtc is not null && PublishAtUtc <= nowUtc;
}

public class NewsGalleryItem
{
    public Guid NewsArticleId { get; set; }
    public Guid MediaAssetId { get; set; }
    public MediaAsset MediaAsset { get; set; } = null!;
    public int SortOrder { get; set; }
}
