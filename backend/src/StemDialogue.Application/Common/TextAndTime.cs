using System.Net;
using System.Text.RegularExpressions;

namespace StemDialogue.Application.Common;

/// <summary>Event times are stored in UTC and shown in Tbilisi time (spec §6).</summary>
public static class TbilisiTime
{
    public const string TimeZoneId = "Asia/Tbilisi";

    private static readonly TimeZoneInfo Zone = Resolve();

    public static DateTimeOffset ToLocal(DateTime utc)
    {
        var value = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTime(new DateTimeOffset(value), Zone);
    }

    public static DateTimeOffset? ToLocal(DateTime? utc) => utc is null ? null : ToLocal(utc.Value);

    public static DateOnly Today(DateTime utcNow) => DateOnly.FromDateTime(ToLocal(utcNow).DateTime);

    /// <summary>UTC range covering a calendar year/month in Tbilisi time.</summary>
    public static (DateTime FromUtc, DateTime ToUtc) MonthRange(int year, int? month)
    {
        var start = new DateTime(year, month ?? 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var end = month is null ? start.AddYears(1) : start.AddMonths(1);
        return (TimeZoneInfo.ConvertTimeToUtc(start, Zone), TimeZoneInfo.ConvertTimeToUtc(end, Zone));
    }

    private static TimeZoneInfo Resolve()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            // Georgia has used UTC+4 without daylight saving since 2005.
            return TimeZoneInfo.CreateCustomTimeZone(TimeZoneId, TimeSpan.FromHours(4), "Tbilisi", "Tbilisi");
        }
    }
}

public static partial class TextUtils
{
    /// <summary>Plain-text excerpt of HTML, for meta descriptions and cards.</summary>
    public static string Excerpt(string? html, int maxLength = 160)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        var text = WebUtility.HtmlDecode(Tags().Replace(html, " "));
        text = Whitespace().Replace(text, " ").Trim();
        if (text.Length <= maxLength) return text;
        var cut = text.LastIndexOf(' ', maxLength - 1);
        return text[..(cut > maxLength / 2 ? cut : maxLength - 1)].TrimEnd(',', '.', ';', ':') + "…";
    }

    public static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    [GeneratedRegex("<[^>]*>")]
    private static partial Regex Tags();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}

/// <summary>Converts video-platform page links into privacy-friendly embed URLs (spec §9).</summary>
public static partial class VideoEmbed
{
    public static string? ToEmbedUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri)) return null;
        var host = uri.Host.ToLowerInvariant();

        if (host is "youtu.be")
            return YouTube(uri.AbsolutePath.Trim('/'));

        if (host.EndsWith("youtube.com", StringComparison.Ordinal))
        {
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            if (query["v"] is { Length: > 0 } v) return YouTube(v);
            var match = YouTubePath().Match(uri.AbsolutePath);
            if (match.Success) return YouTube(match.Groups[1].Value);
            return null;
        }

        if (host.EndsWith("vimeo.com", StringComparison.Ordinal))
        {
            var match = VimeoPath().Match(uri.AbsolutePath);
            return match.Success ? $"https://player.vimeo.com/video/{match.Groups[1].Value}" : null;
        }

        return null;
    }

    private static string? YouTube(string id) =>
        YouTubeId().IsMatch(id) ? $"https://www.youtube-nocookie.com/embed/{id}" : null;

    [GeneratedRegex("^/(?:embed|live|shorts|v)/([A-Za-z0-9_-]{6,20})")]
    private static partial Regex YouTubePath();

    [GeneratedRegex("^[A-Za-z0-9_-]{6,20}$")]
    private static partial Regex YouTubeId();

    [GeneratedRegex(@"^/(?:video/)?(\d{5,12})")]
    private static partial Regex VimeoPath();
}
