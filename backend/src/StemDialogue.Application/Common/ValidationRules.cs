using FluentValidation;

namespace StemDialogue.Application.Common;

public static class ValidationRules
{
    /// <summary>Georgian text is mandatory; both languages are length-limited.</summary>
    public static IRuleBuilderOptions<T, LocalizedTextDto?> RequiredText<T>(this IRuleBuilder<T, LocalizedTextDto?> rule, int maxLength) =>
        rule
            .Must(x => x is not null && !string.IsNullOrWhiteSpace(x.Ka))
            .WithMessage("Georgian text is required.")
            .Must(x => WithinLength(x, maxLength))
            .WithMessage($"Must be at most {maxLength} characters in each language.");

    public static IRuleBuilderOptions<T, LocalizedTextDto?> OptionalText<T>(this IRuleBuilder<T, LocalizedTextDto?> rule, int maxLength) =>
        rule
            .Must(x => WithinLength(x, maxLength))
            .WithMessage($"Must be at most {maxLength} characters in each language.");

    public static IRuleBuilderOptions<T, string?> HttpsUrl<T>(this IRuleBuilder<T, string?> rule) =>
        rule
            .Must(url => string.IsNullOrWhiteSpace(url) || IsHttpsUrl(url))
            .WithMessage("Must be an absolute https:// URL.");

    /// <summary>An https URL or a site-relative path such as <c>/projects/school2university/news</c>.</summary>
    public static IRuleBuilderOptions<T, string?> LinkTarget<T>(this IRuleBuilder<T, string?> rule) =>
        rule
            .Must(url => string.IsNullOrWhiteSpace(url) || IsHttpsUrl(url) || IsSitePath(url) || url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Must be an https:// URL, a mailto: link or a site path starting with '/'.");

    public static IRuleBuilderOptions<T, string?> OptionalSlug<T>(this IRuleBuilder<T, string?> rule) =>
        rule
            .Must(slug => string.IsNullOrWhiteSpace(slug) || Slugifier.IsValid(slug))
            .WithMessage($"Use lowercase latin letters, digits and single dashes (max {Slugifier.MaxLength}).");

    public static bool IsHttpsUrl(string url) =>
        url.Length <= 2048
        && Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps
        && !string.IsNullOrEmpty(uri.Host);

    private static bool IsSitePath(string url) =>
        url.Length <= 512 && url.StartsWith('/') && !url.StartsWith("//", StringComparison.Ordinal) && !url.Contains('\\');

    private static bool WithinLength(LocalizedTextDto? value, int maxLength) =>
        value is null || ((value.Ka?.Length ?? 0) <= maxLength && (value.En?.Length ?? 0) <= maxLength);
}

/// <summary>Body-size limits shared by validators.</summary>
public static class Limits
{
    public const int Title = 200;
    public const int Name = 150;
    public const int Short = 300;
    public const int Summary = 600;
    public const int SeoTitle = 70;
    public const int SeoDescription = 200;
    public const int Html = 100_000;
}
