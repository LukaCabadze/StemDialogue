using StemDialogue.Application.Common;
using StemDialogue.Domain.Projects;

namespace StemDialogue.Application.Projects;

/// <summary>Everything the public layout (header strip, navigation, footer) needs, in one request.</summary>
public sealed record SiteDto(
    string Slug,
    string Code,
    string Name,
    string ShortName,
    string? Tagline,
    ProjectStatus Status,
    DateOnly StartDate,
    DateOnly EndDate,
    MediaDto? Logo,
    FundingDto Funding,
    ContactDto Contact,
    IReadOnlyList<SocialLinkDto> SocialLinks,
    IReadOnlyList<NavItemDto> Navigation,
    SeoDto Seo,
    string Language,
    IReadOnlyList<string> AvailableLanguages);

/// <summary>
/// The funder and the partner universities are separate lists so the UI can never present them
/// in the same role (spec §2.1).
/// </summary>
public sealed record FundingDto(
    string ShortText,
    string FullText,
    IReadOnlyList<PartnerDto> Funders,
    IReadOnlyList<PartnerDto> Partners,
    IReadOnlyList<PartnerDto> Supporters);

public sealed record PartnerDto(
    Guid OrganizationId,
    PartnerRole Role,
    string RoleLabel,
    string Name,
    string ShortName,
    string? Description,
    string? WebsiteUrl,
    MediaDto? Logo,
    MediaDto? LogoOnDark,
    bool ShowInHeader);

public sealed record ContactDto(string? Person, string? Email, string? Phone, string? Address);

public sealed record SocialLinkDto(SocialPlatform Platform, string Url);

public sealed record NavItemDto(string Key, string Label, string Path);

public sealed record PlatformDto(string? DefaultProjectSlug, string? DefaultProjectPath, IReadOnlyList<ProjectSummaryDto> Projects);

public sealed record ProjectSummaryDto(string Slug, string Code, string Name, ProjectStatus Status, DateOnly StartDate, DateOnly EndDate, string Path);
