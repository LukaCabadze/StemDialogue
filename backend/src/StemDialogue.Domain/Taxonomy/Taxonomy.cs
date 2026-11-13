using StemDialogue.Domain.Common;

namespace StemDialogue.Domain.Taxonomy;

/// <summary>Free-form label shared by news, events and resources.</summary>
public class Tag : Entity, IHasSlug
{
    public string Slug { get; set; } = string.Empty;
    public LocalizedText Name { get; set; } = new();
}

/// <summary>STEM direction (მიმართულება) used for event, researcher and resource filters.</summary>
public class Discipline : Entity, IHasSlug
{
    public string Slug { get; set; } = string.Empty;
    public LocalizedText Name { get; set; } = new();
    public int DisplayOrder { get; set; }
}
