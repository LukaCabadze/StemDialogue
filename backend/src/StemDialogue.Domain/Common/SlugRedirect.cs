namespace StemDialogue.Domain.Common;

public enum SluggedContentType
{
    News = 0,
    Event = 1,
    Person = 2,
    Resource = 3,
    SchoolActivity = 4,
}

/// <summary>
/// Keeps an old public URL working after an editor changes a slug (spec §6: an old link must survive).
/// <see cref="ProjectId"/> is null for global content (people).
/// </summary>
public class SlugRedirect : Entity
{
    public SluggedContentType ContentType { get; set; }
    public Guid? ProjectId { get; set; }
    public string OldSlug { get; set; } = string.Empty;
    public Guid TargetId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

/// <summary>Immutable record of an administrative change.</summary>
public class AuditEntry
{
    public long Id { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? ChangesJson { get; set; }
    public DateTime OccurredAtUtc { get; set; }
}
