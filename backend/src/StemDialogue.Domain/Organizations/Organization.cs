using StemDialogue.Domain.Common;
using StemDialogue.Domain.Media;

namespace StemDialogue.Domain.Organizations;

/// <summary>A funder, university or other institution. Shared across projects.</summary>
public class Organization : AuditableEntity
{
    public LocalizedText Name { get; set; } = new();

    /// <summary>Abbreviation, e.g. ბტუ / BTU.</summary>
    public LocalizedText ShortName { get; set; } = new();
    public LocalizedText Description { get; set; } = new();
    public string? WebsiteUrl { get; set; }

    /// <summary>Official logo for light backgrounds. Must be the original artwork (spec §2.1).</summary>
    public Guid? LogoId { get; set; }
    public MediaAsset? Logo { get; set; }

    /// <summary>Official variant for dark backgrounds, if the organization provides one.</summary>
    public Guid? LogoOnDarkId { get; set; }
    public MediaAsset? LogoOnDark { get; set; }
}
