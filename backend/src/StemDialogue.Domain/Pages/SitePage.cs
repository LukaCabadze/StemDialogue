using StemDialogue.Domain.Common;
using StemDialogue.Domain.Media;

namespace StemDialogue.Domain.Pages;

/// <summary>
/// Editable copy for the fixed sections of the public site, so no public text is hard-coded
/// in Angular components (spec §11).
/// </summary>
public class SitePage : AuditableEntity, IHasSeo
{
    public Guid ProjectId { get; set; }

    /// <summary>One of <see cref="PageKeys"/>.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Page heading (and navigation label unless <see cref="NavigationLabel"/> is set).</summary>
    public LocalizedText Title { get; set; } = new();

    /// <summary>Optional shorter label for the menu, e.g. "მთავარი" for the home page.</summary>
    public LocalizedText NavigationLabel { get; set; } = new();

    /// <summary>Intro paragraph (plain text).</summary>
    public LocalizedText Lead { get; set; } = new();

    /// <summary>Main content (sanitized HTML).</summary>
    public LocalizedText Body { get; set; } = new();

    public Guid? HeroImageId { get; set; }
    public MediaAsset? HeroImage { get; set; }

    public LocalizedText PrimaryActionLabel { get; set; } = new();
    public string? PrimaryActionUrl { get; set; }
    public LocalizedText SecondaryActionLabel { get; set; } = new();
    public string? SecondaryActionUrl { get; set; }

    public bool ShowInNavigation { get; set; } = true;
    public int NavigationOrder { get; set; }

    public LocalizedText SeoTitle { get; set; } = new();
    public LocalizedText SeoDescription { get; set; } = new();
    public Guid? ShareImageId { get; set; }
    public MediaAsset? ShareImage { get; set; }
}

/// <summary>Fixed page keys and their public route segments (spec §3).</summary>
public static class PageKeys
{
    public const string Home = "home";
    public const string About = "about";
    public const string News = "news";
    public const string StemSaturdays = "stem-saturdays";
    public const string Team = "team";
    public const string Researchers = "researchers";
    public const string Mentors = "mentors";
    public const string StemAmbassadors = "stem-ambassadors";
    public const string Resources = "resources";
    public const string Contact = "contact";

    /// <summary>Navigation order of the public site.</summary>
    public static readonly IReadOnlyList<string> All =
    [
        Home, About, News, StemSaturdays, Team, Researchers, Mentors, StemAmbassadors, Resources, Contact,
    ];

    public static bool IsValid(string key) => All.Contains(key);

    /// <summary>Route segment relative to the project root; the home page is the root itself.</summary>
    public static string RouteOf(string key) => key == Home ? string.Empty : key;
}
