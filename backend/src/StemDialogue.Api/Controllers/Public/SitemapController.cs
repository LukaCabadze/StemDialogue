using System.Globalization;
using System.Security;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using StemDialogue.Api.Web;
using StemDialogue.Application.Sitemap;

namespace StemDialogue.Api.Controllers.Public;

/// <summary>Search-engine sitemap and the route list for Angular prerendering (spec §11, §15.20).</summary>
[ApiController]
[AllowAnonymous]
[OutputCache(PolicyName = CachePolicies.Public)]
public sealed class SitemapController(SitemapService sitemap, IConfiguration configuration) : ControllerBase
{
    /// <summary>XML sitemap with absolute URLs (base from <c>PublicSite:BaseUrl</c>).</summary>
    [HttpGet("/sitemap.xml")]
    [Produces("application/xml")]
    public async Task<ContentResult> Xml(CancellationToken ct)
    {
        var baseUrl = (configuration["PublicSite:BaseUrl"] ?? "https://stemdialogue.com").TrimEnd('/');
        var xml = new StringBuilder("""<?xml version="1.0" encoding="UTF-8"?>""").AppendLine()
            .AppendLine("""<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">""");

        foreach (var entry in await sitemap.EntriesAsync(ct))
        {
            xml.Append("  <url><loc>").Append(SecurityElement.Escape(baseUrl + entry.Path)).Append("</loc>")
               .Append("<lastmod>").Append(entry.LastModifiedUtc.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append("</lastmod></url>")
               .AppendLine();
        }

        xml.AppendLine("</urlset>");
        return Content(xml.ToString(), "application/xml", Encoding.UTF8);
    }

    /// <summary>Same entries as JSON (relative paths), for the SSG/prerender route list.</summary>
    [HttpGet("api/v1/sitemap")]
    [Produces("application/json")]
    public Task<IReadOnlyList<SitemapEntry>> Json(CancellationToken ct) => sitemap.EntriesAsync(ct);
}
