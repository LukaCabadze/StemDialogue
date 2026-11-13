namespace StemDialogue.Domain.Common;

/// <summary>
/// Bilingual text value. Georgian is the primary, required language; English is optional
/// and falls back to Georgian until it is provided (spec §1: English is added later).
/// Stored as two columns on the owning table.
/// </summary>
public sealed class LocalizedText
{
    public const string Georgian = "ka";
    public const string English = "en";

    public string Ka { get; set; } = string.Empty;
    public string? En { get; set; }

    public LocalizedText()
    {
    }

    public LocalizedText(string ka, string? en = null)
    {
        Ka = ka;
        En = en;
    }

    public bool IsEmpty => string.IsNullOrWhiteSpace(Ka) && string.IsNullOrWhiteSpace(En);

    /// <summary>Returns the text for <paramref name="language"/>, falling back to Georgian.</summary>
    public string Resolve(string language) =>
        language == English && !string.IsNullOrWhiteSpace(En) ? En! : Ka;

    /// <summary>Same as <see cref="Resolve"/> but returns null when no text exists.</summary>
    public string? ResolveOrNull(string language)
    {
        var value = Resolve(language);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>Updates the value in place so EF tracks it as a modification, not a replacement.</summary>
    public void Set(string? ka, string? en)
    {
        Ka = ka?.Trim() ?? string.Empty;
        En = string.IsNullOrWhiteSpace(en) ? null : en.Trim();
    }

    public override string ToString() => Ka;
}
