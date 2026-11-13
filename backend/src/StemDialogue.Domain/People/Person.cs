using StemDialogue.Domain.Common;
using StemDialogue.Domain.Events;
using StemDialogue.Domain.Media;
using StemDialogue.Domain.Taxonomy;

namespace StemDialogue.Domain.People;

/// <summary>
/// Project roles from spec §7. The first four are the team-page sections; Researcher and Mentor have their own lists.
/// </summary>
public enum PersonRole
{
    /// <summary>Official grant staff (შემსრულებელი გუნდი).</summary>
    CoreTeam = 0,

    /// <summary>Invited school-teacher consultants.</summary>
    Consultant = 1,

    /// <summary>Web developers, designers and other specialists.</summary>
    TechnologyTeam = 2,

    /// <summary>Organizational and technical staff joining in stages.</summary>
    OrganizationalStaff = 3,
    Researcher = 4,
    Mentor = 5,
}

public enum PersonLinkType
{
    Website = 0,
    University = 1,
    Orcid = 2,
    GoogleScholar = 3,
    ResearchGate = 4,
    LinkedIn = 5,
    Other = 6,
}

/// <summary>
/// A single profile per human being. One person can hold several roles (researcher and mentor,
/// for example) without duplicated profiles (spec §7.2).
/// </summary>
public class Person : AuditableEntity, IHasSlug, IHasSeo
{
    public string Slug { get; set; } = string.Empty;
    public LocalizedText FullName { get; set; } = new();
    public LocalizedText AcademicTitle { get; set; } = new();
    public LocalizedText Position { get; set; } = new();
    public LocalizedText Organization { get; set; } = new();
    public LocalizedText ShortBio { get; set; } = new();

    /// <summary>Full biography (sanitized HTML).</summary>
    public LocalizedText Biography { get; set; } = new();

    /// <summary>Work done and contribution to the project.</summary>
    public LocalizedText Contribution { get; set; } = new();

    public Guid? PhotoId { get; set; }
    public MediaAsset? Photo { get; set; }

    /// <summary>Internal only, never returned by public endpoints.</summary>
    public string? Email { get; set; }

    public PublicationStatus Status { get; set; } = PublicationStatus.Draft;
    public int DisplayOrder { get; set; }

    public List<PersonRoleAssignment> Roles { get; set; } = [];
    public List<PersonLink> Links { get; set; } = [];
    public List<Discipline> Disciplines { get; set; } = [];
    public List<EventParticipant> Events { get; set; } = [];

    public LocalizedText SeoTitle { get; set; } = new();
    public LocalizedText SeoDescription { get; set; } = new();
    public Guid? ShareImageId { get; set; }
    public MediaAsset? ShareImage { get; set; }
}

/// <summary>
/// A role held in one project for a period. Ended roles are kept so a former member's
/// contribution stays in the archive (spec §7.3).
/// </summary>
public class PersonRoleAssignment : Entity
{
    public Guid PersonId { get; set; }
    public Guid ProjectId { get; set; }
    public PersonRole Role { get; set; }

    /// <summary>Position within the role, e.g. "პროექტის ხელმძღვანელი".</summary>
    public LocalizedText Title { get; set; } = new();
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int DisplayOrder { get; set; }

    public bool IsCurrent(DateOnly today) => EndDate is null || EndDate >= today;

    public static bool IsTeamRole(PersonRole role) => role is
        PersonRole.CoreTeam or PersonRole.Consultant or PersonRole.TechnologyTeam or PersonRole.OrganizationalStaff;
}

public class PersonLink : Entity
{
    public Guid PersonId { get; set; }
    public PersonLinkType Type { get; set; }
    public string Url { get; set; } = string.Empty;
    public LocalizedText Label { get; set; } = new();
    public int SortOrder { get; set; }
}
