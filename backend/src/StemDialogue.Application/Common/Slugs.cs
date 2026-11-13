using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace StemDialogue.Application.Common;

/// <summary>
/// Builds readable, ASCII URL slugs. Georgian is transliterated with the national romanization
/// system, e.g. "STEM შაბათობა: ფიზიკა" → "stem-shabatoba-pizika".
/// </summary>
public static partial class Slugifier
{
    public const int MaxLength = 120;

    private static readonly Dictionary<char, string> Georgian = new()
    {
        ['ა'] = "a", ['ბ'] = "b", ['გ'] = "g", ['დ'] = "d", ['ე'] = "e", ['ვ'] = "v", ['ზ'] = "z",
        ['თ'] = "t", ['ი'] = "i", ['კ'] = "k", ['ლ'] = "l", ['მ'] = "m", ['ნ'] = "n", ['ო'] = "o",
        ['პ'] = "p", ['ჟ'] = "zh", ['რ'] = "r", ['ს'] = "s", ['ტ'] = "t", ['უ'] = "u", ['ფ'] = "p",
        ['ქ'] = "k", ['ღ'] = "gh", ['ყ'] = "q", ['შ'] = "sh", ['ჩ'] = "ch", ['ც'] = "ts", ['ძ'] = "dz",
        ['წ'] = "ts", ['ჭ'] = "ch", ['ხ'] = "kh", ['ჯ'] = "j", ['ჰ'] = "h",
    };

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex ValidPattern();

    public static bool IsValid(string? slug) =>
        !string.IsNullOrEmpty(slug) && slug.Length <= MaxLength && ValidPattern().IsMatch(slug);

    public static string From(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var builder = new StringBuilder(text.Length);
        foreach (var raw in text.Trim().Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(raw) == UnicodeCategory.NonSpacingMark) continue;
            var c = char.ToLowerInvariant(raw);
            if (Georgian.TryGetValue(c, out var latin)) builder.Append(latin);
            else if (c is >= 'a' and <= 'z' or >= '0' and <= '9') builder.Append(c);
            else builder.Append('-');
        }

        var slug = CollapseDashes().Replace(builder.ToString(), "-").Trim('-');
        if (slug.Length > MaxLength) slug = slug[..MaxLength].TrimEnd('-');
        return slug;
    }

    /// <summary>
    /// Returns <paramref name="candidate"/> or the first free variant (<c>slug-2</c>, <c>slug-3</c> …).
    /// </summary>
    public static async Task<string> EnsureUniqueAsync(string candidate, Func<string, Task<bool>> isTaken)
    {
        if (string.IsNullOrEmpty(candidate)) candidate = "item";
        if (!await isTaken(candidate)) return candidate;

        for (var n = 2; n < 1000; n++)
        {
            var suffix = "-" + n.ToString(CultureInfo.InvariantCulture);
            var stem = candidate.Length + suffix.Length > MaxLength ? candidate[..(MaxLength - suffix.Length)] : candidate;
            var next = stem.TrimEnd('-') + suffix;
            if (!await isTaken(next)) return next;
        }

        return candidate + "-" + Guid.NewGuid().ToString("N")[..8];
    }

    [GeneratedRegex("-{2,}")]
    private static partial Regex CollapseDashes();
}
