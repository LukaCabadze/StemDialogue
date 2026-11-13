using Ganss.Xss;
using StemDialogue.Application.Abstractions;

namespace StemDialogue.Infrastructure.Html;

/// <summary>
/// Allow-list sanitizer for rich text from the CMS editor. Only structural markup survives:
/// no scripts, styles, forms, iframes or event handlers.
/// </summary>
public sealed class HtmlContentSanitizer : IHtmlContentSanitizer
{
    private static readonly string[] Tags =
    [
        "p", "br", "hr", "h2", "h3", "h4", "strong", "b", "em", "i", "u", "s", "sub", "sup", "mark", "small",
        "blockquote", "ul", "ol", "li", "a", "img", "figure", "figcaption", "table", "thead", "tbody", "tr", "th", "td",
        "caption", "code", "pre", "span", "abbr",
    ];

    private static readonly string[] Attributes =
    [
        "href", "src", "alt", "title", "width", "height", "colspan", "rowspan", "scope", "target", "rel", "lang", "dir",
    ];

    private readonly HtmlSanitizer _sanitizer;

    public HtmlContentSanitizer()
    {
        _sanitizer = new HtmlSanitizer();
        _sanitizer.AllowedTags.Clear();
        foreach (var tag in Tags) _sanitizer.AllowedTags.Add(tag);

        _sanitizer.AllowedAttributes.Clear();
        foreach (var attribute in Attributes) _sanitizer.AllowedAttributes.Add(attribute);

        _sanitizer.AllowedCssProperties.Clear();
        _sanitizer.AllowedAtRules.Clear();

        _sanitizer.AllowedSchemes.Clear();
        _sanitizer.AllowedSchemes.Add("https");
        _sanitizer.AllowedSchemes.Add("http");
        _sanitizer.AllowedSchemes.Add("mailto");

        // Links opening a new tab must not get a handle to this window.
        _sanitizer.PostProcessNode += (_, e) =>
        {
            if (e.Node is AngleSharp.Dom.IElement { TagName: "A" } link && link.GetAttribute("target") == "_blank")
                link.SetAttribute("rel", "noopener noreferrer");
        };
    }

    public string Sanitize(string? html) => string.IsNullOrWhiteSpace(html) ? string.Empty : _sanitizer.Sanitize(html).Trim();
}
