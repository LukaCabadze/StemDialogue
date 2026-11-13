using FluentValidation;
using StemDialogue.Application.Common;
using StemDialogue.Application.Projects;

namespace StemDialogue.Application.Pages;

// ---------- Public ----------

public sealed record ActionDto(string Label, string Url);

public sealed record PageDto(
    string Key,
    string Title,
    string? Lead,
    string? BodyHtml,
    MediaDto? HeroImage,
    ActionDto? PrimaryAction,
    ActionDto? SecondaryAction,
    SeoDto Seo,
    string Path);

public enum MilestoneState
{
    Upcoming = 0,
    Current = 1,
    Done = 2,
}

public sealed record MilestoneDto(
    Guid Id,
    string Title,
    string? Description,
    string? PeriodLabel,
    string? Location,
    DateOnly? StartDate,
    DateOnly? EndDate,
    MilestoneState State);

public sealed record AboutDto(PageDto Page, IReadOnlyList<MilestoneDto> Milestones, FundingDto Funding);

public sealed record ContactPageDto(PageDto Page, ContactDto Contact, IReadOnlyList<SocialLinkDto> SocialLinks, FundingDto Funding);

// ---------- Admin ----------

public sealed record PageUpdateRequest(
    LocalizedTextDto Title,
    LocalizedTextDto? NavigationLabel,
    LocalizedTextDto? Lead,
    LocalizedTextDto? Body,
    Guid? HeroImageId,
    LocalizedTextDto? PrimaryActionLabel,
    string? PrimaryActionUrl,
    LocalizedTextDto? SecondaryActionLabel,
    string? SecondaryActionUrl,
    bool ShowInNavigation,
    int NavigationOrder,
    LocalizedTextDto? SeoTitle,
    LocalizedTextDto? SeoDescription,
    Guid? ShareImageId,
    Guid ConcurrencyStamp);

public sealed record PageAdminListItem(string Key, string Title, bool ShowInNavigation, int NavigationOrder, DateTime UpdatedAtUtc, string? UpdatedBy, string PublicPath);

public sealed record PageAdminDto(
    Guid Id,
    string Key,
    LocalizedTextDto Title,
    LocalizedTextDto NavigationLabel,
    LocalizedTextDto Lead,
    LocalizedTextDto Body,
    AdminMediaRef? HeroImage,
    LocalizedTextDto PrimaryActionLabel,
    string? PrimaryActionUrl,
    LocalizedTextDto SecondaryActionLabel,
    string? SecondaryActionUrl,
    bool ShowInNavigation,
    int NavigationOrder,
    LocalizedTextDto SeoTitle,
    LocalizedTextDto SeoDescription,
    AdminMediaRef? ShareImage,
    string PublicPath,
    AuditInfo Audit);

public sealed record MilestoneUpsertRequest(
    LocalizedTextDto Title,
    LocalizedTextDto? Description,
    LocalizedTextDto? PeriodLabel,
    LocalizedTextDto? Location,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsVisible,
    Guid? ConcurrencyStamp);

public sealed record MilestoneAdminDto(
    Guid Id,
    LocalizedTextDto Title,
    LocalizedTextDto Description,
    LocalizedTextDto PeriodLabel,
    LocalizedTextDto Location,
    DateOnly? StartDate,
    DateOnly? EndDate,
    int DisplayOrder,
    bool IsVisible,
    AuditInfo Audit);

/// <summary>New order for a list of items; every existing id must be present exactly once.</summary>
public sealed record ReorderRequest(IReadOnlyList<Guid> Ids);

public sealed class PageUpdateValidator : AbstractValidator<PageUpdateRequest>
{
    public PageUpdateValidator()
    {
        RuleFor(x => x.Title).RequiredText(Limits.Title);
        RuleFor(x => x.NavigationLabel).OptionalText(40);
        RuleFor(x => x.Lead).OptionalText(1000);
        RuleFor(x => x.Body).OptionalText(Limits.Html);
        RuleFor(x => x.PrimaryActionLabel).OptionalText(60);
        RuleFor(x => x.PrimaryActionUrl).LinkTarget();
        RuleFor(x => x.SecondaryActionLabel).OptionalText(60);
        RuleFor(x => x.SecondaryActionUrl).LinkTarget();
        RuleFor(x => x.PrimaryActionUrl).NotEmpty()
            .When(x => x.PrimaryActionLabel is not null && !string.IsNullOrWhiteSpace(x.PrimaryActionLabel.Ka))
            .WithMessage("A button needs a link.");
        RuleFor(x => x.SecondaryActionUrl).NotEmpty()
            .When(x => x.SecondaryActionLabel is not null && !string.IsNullOrWhiteSpace(x.SecondaryActionLabel.Ka))
            .WithMessage("A button needs a link.");
        RuleFor(x => x.NavigationOrder).InclusiveBetween(0, 100);
        RuleFor(x => x.SeoTitle).OptionalText(Limits.SeoTitle);
        RuleFor(x => x.SeoDescription).OptionalText(Limits.SeoDescription);
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public sealed class MilestoneUpsertValidator : AbstractValidator<MilestoneUpsertRequest>
{
    public MilestoneUpsertValidator()
    {
        RuleFor(x => x.Title).RequiredText(Limits.Title);
        RuleFor(x => x.Description).OptionalText(1000);
        RuleFor(x => x.PeriodLabel).OptionalText(80);
        RuleFor(x => x.Location).OptionalText(Limits.Short);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate!.Value)
            .When(x => x.StartDate is not null && x.EndDate is not null)
            .WithMessage("The end date must not be before the start date.");
    }
}

public sealed class ReorderValidator : AbstractValidator<ReorderRequest>
{
    public ReorderValidator()
    {
        RuleFor(x => x.Ids).NotNull().Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Ids must be unique.");
    }
}
