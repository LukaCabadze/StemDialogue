using StemDialogue.Domain.Common;
using StemDialogue.Domain.Media;

namespace StemDialogue.Domain.Ambassadors;

/// <summary>
/// A school initiative run by STEM ambassadors (spec §8). Deliberately has no participant profiles:
/// identifiable material about minors is published only when consent is recorded.
/// </summary>
public class SchoolActivity : AuditableEntity, IHasSlug
{
    public Guid ProjectId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public LocalizedText Title { get; set; } = new();
    public LocalizedText Summary { get; set; } = new();

    /// <summary>Report text (sanitized HTML).</summary>
    public LocalizedText Body { get; set; } = new();
    public LocalizedText SchoolName { get; set; } = new();
    public LocalizedText Region { get; set; } = new();
    public DateOnly? ActivityDate { get; set; }

    public Guid? CoverImageId { get; set; }
    public MediaAsset? CoverImage { get; set; }
    public List<SchoolActivityGalleryItem> Gallery { get; set; } = [];

    public PublicationStatus Status { get; set; } = PublicationStatus.Draft;

    /// <summary>Confirms that the legal basis and permissions for the attached photos exist.</summary>
    public bool MediaConsentConfirmed { get; set; }

    /// <summary>Internal reference to the consent documents. Never public.</summary>
    public string? ConsentReference { get; set; }

    public bool HasImagery => CoverImageId is not null || Gallery.Count > 0;
}

public class SchoolActivityGalleryItem
{
    public Guid SchoolActivityId { get; set; }
    public Guid MediaAssetId { get; set; }
    public MediaAsset MediaAsset { get; set; } = null!;
    public int SortOrder { get; set; }
}
