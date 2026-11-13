namespace StemDialogue.Domain.Common;

/// <summary>Base type for every persisted aggregate and child entity.</summary>
public abstract class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}

/// <summary>
/// Entity with authoring metadata and an optimistic-concurrency stamp.
/// Timestamps, authors and the stamp are maintained by the persistence layer, never by callers.
/// </summary>
public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    /// <summary>Changes on every save. Clients send it back on update to detect lost updates.</summary>
    public Guid ConcurrencyStamp { get; set; } = Guid.NewGuid();
}

/// <summary>Editorial visibility, independent of an event's lifecycle.</summary>
public enum PublicationStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2,
}

/// <summary>Search-engine and social-preview overrides. Empty values fall back to the content itself.</summary>
public interface IHasSeo
{
    LocalizedText SeoTitle { get; }
    LocalizedText SeoDescription { get; }
    Guid? ShareImageId { get; }
}

/// <summary>Content addressed by a public, human-readable URL segment.</summary>
public interface IHasSlug
{
    string Slug { get; set; }
}
