using System.Text;
using StemDialogue.Application.Common;
using StemDialogue.Application.Media;
using StemDialogue.Domain.Common;
using StemDialogue.Domain.Events;

namespace StemDialogue.Tests;

public sealed class SlugifierTests
{
    [Theory]
    [InlineData("STEM შაბათობა: ფიზიკა", "stem-shabatoba-pizika")]
    [InlineData("ქეთი ქეთელაური", "keti-ketelauri")]
    [InlineData("ჭადრაკი და ღრუბლები", "chadraki-da-ghrublebi")]
    [InlineData("  Hello,   World!  ", "hello-world")]
    [InlineData("Café 2026", "cafe-2026")]
    public void Transliterates_georgian_and_normalizes(string input, string expected) =>
        Assert.Equal(expected, Slugifier.From(input));

    [Theory]
    [InlineData("valid-slug-1", true)]
    [InlineData("Invalid", false)]
    [InlineData("double--dash", false)]
    [InlineData("-leading", false)]
    public void Validates_slugs(string slug, bool valid) => Assert.Equal(valid, Slugifier.IsValid(slug));

    [Fact]
    public async Task Finds_the_next_free_slug()
    {
        var taken = new HashSet<string> { "news", "news-2" };
        Assert.Equal("news-3", await Slugifier.EnsureUniqueAsync("news", s => Task.FromResult(taken.Contains(s))));
    }
}

public sealed class VideoEmbedTests
{
    [Theory]
    [InlineData("https://www.youtube.com/watch?v=abcdefghijk", "https://www.youtube-nocookie.com/embed/abcdefghijk")]
    [InlineData("https://youtu.be/abcdefghijk", "https://www.youtube-nocookie.com/embed/abcdefghijk")]
    [InlineData("https://www.youtube.com/live/abcdefghijk", "https://www.youtube-nocookie.com/embed/abcdefghijk")]
    [InlineData("https://vimeo.com/123456789", "https://player.vimeo.com/video/123456789")]
    [InlineData("https://example.com/video", null)]
    public void Builds_privacy_friendly_embeds(string url, string? expected) => Assert.Equal(expected, VideoEmbed.ToEmbedUrl(url));
}

public sealed class MediaInspectorTests
{
    private static readonly MediaOptions Options = new();

    [Fact]
    public void Accepts_svg_logo_with_embedded_raster_image()
    {
        var svg = Encoding.UTF8.GetBytes("<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\"><image xlink:href=\"data:image/png;base64,AAAA\"/></svg>");
        var (file, error) = MediaInspector.Inspect("logo.svg", svg, svg.Length, Options);
        Assert.Null(error);
        Assert.Equal("image/svg+xml", file!.ContentType);
    }

    [Theory]
    [InlineData("<svg onload=\"alert(1)\"></svg>")]
    [InlineData("<svg><a href=\"javascript:alert(1)\">x</a></svg>")]
    [InlineData("<svg><image href=\"https://evil.example/x.png\"/></svg>")]
    public void Rejects_active_svg(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var (file, error) = MediaInspector.Inspect("x.svg", bytes, bytes.Length, Options);
        Assert.Null(file);
        Assert.NotNull(error);
    }

    [Fact]
    public void Rejects_oversized_images()
    {
        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var (_, error) = MediaInspector.Inspect("big.png", png, Options.MaxImageBytes + 1, Options);
        Assert.Contains("too large", error);
    }
}

public sealed class EventRulesTests
{
    private static readonly DateTime Now = new(2026, 11, 10, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Postponing_keeps_the_original_date()
    {
        var e = new Event { StartsAtUtc = Now.AddDays(2) };
        e.Postpone(Now.AddDays(9), null, "გადაიდო", null, Now);

        Assert.Equal(EventStatus.Postponed, e.Status);
        Assert.Equal(Now.AddDays(2), e.OriginalStartsAtUtc);
        Assert.Equal("გადაიდო", e.StatusNote.Ka);
    }

    [Fact]
    public void Cancelled_event_must_be_restored_before_completion()
    {
        var e = new Event { StartsAtUtc = Now.AddDays(-1) };
        e.Cancel("გაუქმდა", null);
        Assert.Throws<DomainRuleException>(() => e.Complete(Now));

        e.Restore();
        e.Complete(Now);
        Assert.Equal(EventStatus.Completed, e.Status);
    }

    [Fact]
    public void Localized_text_falls_back_to_georgian()
    {
        var text = new LocalizedText("ქართული", null);
        Assert.Equal("ქართული", text.Resolve(LocalizedText.English));
        text.Set("ქართული", "English");
        Assert.Equal("English", text.Resolve(LocalizedText.English));
    }
}
