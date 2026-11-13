using FluentValidation;
using StemDialogue.Application.Common;
using StemDialogue.Application.Events;
using StemDialogue.Application.Pages;
using StemDialogue.Application.Resources;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.People;

namespace StemDialogue.Application.People;

// ---------- Public ----------

public sealed class PeopleQuery : PageQuery
{
    public string? Q { get; set; }
    public string? Discipline { get; set; }

    /// <summary>Include people whose role has ended (archive).</summary>
    public bool IncludeFormer { get; set; } = true;
}

/// <summary>Compact person reference used on event and resource cards.</summary>
public sealed record PersonRefDto(
    string Slug,
    string FullName,
    string? AcademicTitle,
    string? Organization,
    MediaDto? Photo,
    string? Role,
    string Path);

public sealed record PersonCardDto(
    string Slug,
    string FullName,
    string? AcademicTitle,
    string? Position,
    string? Organization,
    string? ShortBio,
    MediaDto? Photo,
    IReadOnlyList<PersonRole> Roles,
    IReadOnlyList<TermDto> Disciplines,
    bool IsCurrent,
    string Path);

public sealed record TeamDto(PageDto? Page, IReadOnlyList<TeamSectionDto> Sections);

/// <summary>
/// One of the four team-page sections (spec §7.1). <see cref="IsOfficialGrantStaff"/> separates
/// the official grant personnel from additionally engaged specialists.
/// </summary>
public sealed record TeamSectionDto(
    PersonRole Role,
    string Title,
    bool IsOfficialGrantStaff,
    IReadOnlyList<TeamMemberDto> Current,
    IReadOnlyList<TeamMemberDto> Former);

public sealed record TeamMemberDto(
    string Slug,
    string FullName,
    string? RoleTitle,
    string? AcademicTitle,
    string? Organization,
    string? ShortBio,
    MediaDto? Photo,
    DateOnly? Since,
    DateOnly? Until,
    string Path);

public sealed record PersonRoleDto(PersonRole Role, string RoleLabel, string? Title, DateOnly? Since, DateOnly? Until, bool IsCurrent);

public sealed record PersonLinkDto(PersonLinkType Type, string Url, string? Label);

public sealed record PersonDetailDto(
    string Slug,
    string FullName,
    string? AcademicTitle,
    string? Position,
    string? Organization,
    string? ShortBio,
    string? BiographyHtml,
    string? Contribution,
    MediaDto? Photo,
    IReadOnlyList<PersonRoleDto> Roles,
    IReadOnlyList<PersonLinkDto> Links,
    IReadOnlyList<TermDto> Disciplines,
    IReadOnlyList<EventCardDto> UpcomingEvents,
    IReadOnlyList<EventCardDto> PastEvents,
    IReadOnlyList<ResourceCardDto> Resources,
    SeoDto Seo,
    string Path);

// ---------- Admin ----------

public sealed class PeopleAdminQuery : PageQuery
{
    public PersonRole? Role { get; set; }
    public PublicationStatus? Status { get; set; }
    public string? Search { get; set; }
}

public sealed record PersonRoleInput(
    Guid ProjectId,
    PersonRole Role,
    LocalizedTextDto? Title,
    DateOnly? StartDate,
    DateOnly? EndDate,
    int DisplayOrder);

public sealed record PersonLinkInput(PersonLinkType Type, string Url, LocalizedTextDto? Label);

public sealed record PersonUpsertRequest(
    LocalizedTextDto FullName,
    string? Slug,
    LocalizedTextDto? AcademicTitle,
    LocalizedTextDto? Position,
    LocalizedTextDto? Organization,
    LocalizedTextDto? ShortBio,
    LocalizedTextDto? Biography,
    LocalizedTextDto? Contribution,
    Guid? PhotoId,
    string? Email,
    PublicationStatus Status,
    int DisplayOrder,
    IReadOnlyList<PersonRoleInput>? Roles,
    IReadOnlyList<PersonLinkInput>? Links,
    IReadOnlyList<Guid>? DisciplineIds,
    LocalizedTextDto? SeoTitle,
    LocalizedTextDto? SeoDescription,
    Guid? ShareImageId,
    Guid? ConcurrencyStamp);

/// <summary>Changes only a person's roles/categories (spec §10: change a team member's category or role).</summary>
public sealed record PersonRolesRequest(IReadOnlyList<PersonRoleInput> Roles, Guid ConcurrencyStamp);

public sealed record PersonAdminListItem(
    Guid Id,
    string Slug,
    string FullName,
    string? Organization,
    PublicationStatus Status,
    IReadOnlyList<PersonRole> Roles,
    bool HasPhoto,
    int EventCount,
    DateTime UpdatedAtUtc,
    string? UpdatedBy);

public sealed record PersonRoleAdminDto(
    Guid Id,
    Guid ProjectId,
    PersonRole Role,
    LocalizedTextDto Title,
    DateOnly? StartDate,
    DateOnly? EndDate,
    int DisplayOrder,
    bool IsCurrent);

public sealed record PersonAdminDto(
    Guid Id,
    string Slug,
    LocalizedTextDto FullName,
    LocalizedTextDto AcademicTitle,
    LocalizedTextDto Position,
    LocalizedTextDto Organization,
    LocalizedTextDto ShortBio,
    LocalizedTextDto Biography,
    LocalizedTextDto Contribution,
    AdminMediaRef? Photo,
    string? Email,
    PublicationStatus Status,
    int DisplayOrder,
    IReadOnlyList<PersonRoleAdminDto> Roles,
    IReadOnlyList<PersonLinkInput> Links,
    IReadOnlyList<Guid> DisciplineIds,
    LocalizedTextDto SeoTitle,
    LocalizedTextDto SeoDescription,
    AdminMediaRef? ShareImage,
    IReadOnlyList<string> PublicPaths,
    AuditInfo Audit);

public sealed class PersonRoleInputValidator : AbstractValidator<PersonRoleInput>
{
    public PersonRoleInputValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Role).IsInEnum();
        RuleFor(x => x.Title).OptionalText(Limits.Name);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate!.Value)
            .When(x => x.StartDate is not null && x.EndDate is not null)
            .WithMessage("The end date must not be before the start date.");
    }
}

internal static class PersonRoleRules
{
    public static bool Unique(IReadOnlyList<PersonRoleInput>? roles) =>
        roles is null || roles.Select(r => (r.ProjectId, r.Role)).Distinct().Count() == roles.Count;
}

public sealed class PersonUpsertValidator : AbstractValidator<PersonUpsertRequest>
{
    public PersonUpsertValidator()
    {
        RuleFor(x => x.FullName).RequiredText(Limits.Name);
        RuleFor(x => x.Slug).OptionalSlug();
        RuleFor(x => x.AcademicTitle).OptionalText(Limits.Name);
        RuleFor(x => x.Position).OptionalText(Limits.Short);
        RuleFor(x => x.Organization).OptionalText(Limits.Short);
        RuleFor(x => x.ShortBio).OptionalText(Limits.Summary);
        RuleFor(x => x.Biography).OptionalText(Limits.Html);
        RuleFor(x => x.Contribution).OptionalText(2000);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(254).When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Roles).Must(PersonRoleRules.Unique).WithMessage("Each role can be assigned once per project.");
        RuleForEach(x => x.Roles).SetValidator(new PersonRoleInputValidator());
        RuleFor(x => x.Links).Must(l => l is null || l.Count <= 10).WithMessage("At most 10 links.");
        RuleForEach(x => x.Links).ChildRules(l =>
        {
            l.RuleFor(x => x.Type).IsInEnum();
            l.RuleFor(x => x.Url).NotEmpty().HttpsUrl();
            l.RuleFor(x => x.Label).OptionalText(80);
        });
        RuleFor(x => x.SeoTitle).OptionalText(Limits.SeoTitle);
        RuleFor(x => x.SeoDescription).OptionalText(Limits.SeoDescription);
    }
}

public sealed class PersonRolesValidator : AbstractValidator<PersonRolesRequest>
{
    public PersonRolesValidator()
    {
        RuleFor(x => x.Roles).NotNull().Must(PersonRoleRules.Unique).WithMessage("Each role can be assigned once per project.");
        RuleForEach(x => x.Roles).SetValidator(new PersonRoleInputValidator());
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}
