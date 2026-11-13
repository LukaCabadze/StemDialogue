using StemDialogue.Domain.Common;

namespace StemDialogue.Application.Common;

/// <summary>Bilingual value as exchanged with the admin panel.</summary>
public sealed record LocalizedTextDto(string? Ka, string? En)
{
    public static readonly LocalizedTextDto Empty = new(null, null);
}

public static class LocalizedTextMapping
{
    public static LocalizedTextDto ToDto(this LocalizedText text) => new(text.Ka, text.En);

    /// <summary>Writes the DTO into the tracked value (in place).</summary>
    public static void Apply(this LocalizedText target, LocalizedTextDto? source) => target.Set(source?.Ka, source?.En);

    /// <summary>Writes sanitized HTML into the tracked value.</summary>
    public static void ApplyHtml(this LocalizedText target, LocalizedTextDto? source, Abstractions.IHtmlContentSanitizer sanitizer) =>
        target.Set(
            string.IsNullOrWhiteSpace(source?.Ka) ? null : sanitizer.Sanitize(source.Ka),
            string.IsNullOrWhiteSpace(source?.En) ? null : sanitizer.Sanitize(source.En));

    public static LocalizedText ToValue(this LocalizedTextDto? source)
    {
        var value = new LocalizedText();
        value.Apply(source);
        return value;
    }
}
