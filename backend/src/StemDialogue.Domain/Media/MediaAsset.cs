using StemDialogue.Domain.Common;

namespace StemDialogue.Domain.Media;

public enum MediaKind
{
    Image = 0,
    Document = 1,
    Presentation = 2,
    Spreadsheet = 3,
}

/// <summary>
/// An uploaded file. Large video is not uploaded: it is linked from a video platform (spec §9).
/// </summary>
public class MediaAsset : AuditableEntity
{
    /// <summary>Path relative to the storage root, e.g. <c>2026/11/3f2c….png</c>.</summary>
    public string StoragePath { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public MediaKind Kind { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string Sha256 { get; set; } = string.Empty;

    /// <summary>Required for images used publicly (WCAG 2.2 AA, spec §12).</summary>
    public LocalizedText AltText { get; set; } = new();
    public LocalizedText Caption { get; set; } = new();
    public LocalizedText Credit { get; set; } = new();
}
