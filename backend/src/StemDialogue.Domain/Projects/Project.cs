using StemDialogue.Domain.Common;
using StemDialogue.Domain.Media;
using StemDialogue.Domain.Organizations;

namespace StemDialogue.Domain.Projects;

public enum ProjectStatus
{
    Upcoming = 0,
    Active = 1,
    Completed = 2,
}

/// <summary>
/// One grant project with its own public site under <c>/projects/{slug}</c>.
/// Every piece of project content is scoped by <see cref="Entity.Id"/>, so the future multi-project
/// STEM Dialogue platform can be added without changing this project's URLs or archive (spec §1, §11).
/// </summary>
public class Project : AuditableEntity, IHasSlug, IHasSeo
{
    public string Slug { get; set; } = string.Empty;

    /// <summary>Grant code, e.g. SPG-26-185.</summary>
    public string Code { get; set; } = string.Empty;
    public LocalizedText Name { get; set; } = new();
    public LocalizedText ShortName { get; set; } = new();
    public LocalizedText Tagline { get; set; } = new();
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    /// <summary>The project the bare domain temporarily redirects to (spec §3).</summary>
    public bool IsDefault { get; set; }

    /// <summary>Short funding line in the header strip (spec §2.1, configurable separately).</summary>
    public LocalizedText FundingShortText { get; set; } = new();

    /// <summary>Full official funding statement in the footer.</summary>
    public LocalizedText FundingFullText { get; set; } = new();

    public Guid? LogoId { get; set; }
    public MediaAsset? Logo { get; set; }

    public LocalizedText ContactPerson { get; set; } = new();
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public LocalizedText ContactAddress { get; set; } = new();

    public LocalizedText SeoTitle { get; set; } = new();
    public LocalizedText SeoDescription { get; set; } = new();
    public Guid? ShareImageId { get; set; }
    public MediaAsset? ShareImage { get; set; }

    public List<ProjectPartner> Partners { get; set; } = [];
    public List<SocialLink> SocialLinks { get; set; } = [];
}

/// <summary>
/// Distinguishes the funder from partner universities, so the three are never shown in the same role (spec §2.1).
/// </summary>
public enum PartnerRole
{
    Funder = 0,
    PartnerUniversity = 1,
    Supporter = 2,
}

public class ProjectPartner
{
    public Guid ProjectId { get; set; }
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public PartnerRole Role { get; set; }

    /// <summary>Optional display label overriding the default role label.</summary>
    public LocalizedText RoleLabel { get; set; } = new();
    public int DisplayOrder { get; set; }

    /// <summary>Whether the organization is named in the compact header strip.</summary>
    public bool ShowInHeader { get; set; } = true;
}

public enum SocialPlatform
{
    Facebook = 0,
    Instagram = 1,
    YouTube = 2,
    LinkedIn = 3,
    X = 4,
    TikTok = 5,
    Website = 6,
    Other = 7,
}

public class SocialLink : Entity
{
    public Guid ProjectId { get; set; }
    public SocialPlatform Platform { get; set; }
    public string Url { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

/// <summary>A stage on the project timeline (About page, spec §1: forum in May 2027, final event in June 2027).</summary>
public class ProjectMilestone : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public LocalizedText Title { get; set; } = new();
    public LocalizedText Description { get; set; } = new();

    /// <summary>Human period label, e.g. "მაისი 2027". Dates below drive ordering and status.</summary>
    public LocalizedText PeriodLabel { get; set; } = new();
    public LocalizedText Location { get; set; } = new();
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; } = true;
}
