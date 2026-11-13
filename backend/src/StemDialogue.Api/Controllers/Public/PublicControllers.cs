using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using StemDialogue.Api.Web;
using StemDialogue.Application.Ambassadors;
using StemDialogue.Application.Common;
using StemDialogue.Application.Events;
using StemDialogue.Application.Home;
using StemDialogue.Application.News;
using StemDialogue.Application.Pages;
using StemDialogue.Application.People;
using StemDialogue.Application.Projects;
using StemDialogue.Application.Resources;
using StemDialogue.Domain.Pages;

namespace StemDialogue.Api.Controllers.Public;

/// <summary>
/// Base for the read-only public API. Anonymous, cached for 60 s and evicted on every CMS change.
/// Every endpoint accepts <c>?lang=en</c>; Georgian is the default.
/// </summary>
[ApiController]
[AllowAnonymous]
[OutputCache(PolicyName = CachePolicies.Public)]
[Produces("application/json")]
public abstract class PublicApiController : ControllerBase;

/// <summary>Platform entry point: which project the bare domain redirects to (spec §3).</summary>
[Route("api/v1/site")]
public sealed class PlatformController(SiteService site) : PublicApiController
{
    [HttpGet]
    public Task<PlatformDto> Get(CancellationToken ct) => site.GetPlatformAsync(ct);
}

/// <summary>Project shell, home page and fixed pages.</summary>
[Route("api/v1/projects/{projectSlug}")]
public sealed class ProjectSiteController(SiteService site, HomeService home, PublicPageService pages) : PublicApiController
{
    /// <summary>Header, funding strip, navigation, footer and default SEO. Load once per visit.</summary>
    [HttpGet]
    public Task<SiteDto> Site(string projectSlug, CancellationToken ct) => site.GetSiteAsync(projectSlug, ct);

    /// <summary>All home-page blocks (spec §4). Empty blocks are null or empty lists.</summary>
    [HttpGet("home")]
    public Task<HomeDto> Home(string projectSlug, CancellationToken ct) => home.GetAsync(projectSlug, ct);

    /// <summary>About page with project stages and the funder/partners block.</summary>
    [HttpGet("about")]
    public Task<AboutDto> About(string projectSlug, CancellationToken ct) => pages.AboutAsync(projectSlug, ct);

    [HttpGet("contact")]
    public Task<ContactPageDto> Contact(string projectSlug, CancellationToken ct) => pages.ContactAsync(projectSlug, ct);

    /// <summary>Intro copy and SEO for any section page, e.g. <c>news</c>, <c>researchers</c>.</summary>
    [HttpGet("pages/{key}")]
    public Task<PageDto> Page(string projectSlug, string key, CancellationToken ct) => pages.GetAsync(projectSlug, key, ct);

    /// <summary>Project timeline (stages) shown on the About page.</summary>
    [HttpGet("milestones")]
    public async Task<IReadOnlyList<MilestoneDto>> Milestones(string projectSlug, [FromServices] ProjectResolver projects, CancellationToken ct) =>
        await pages.MilestonesAsync(await projects.ResolveAsync(projectSlug, ct), ct);
}

[Route("api/v1/projects/{projectSlug}/news")]
public sealed class NewsController(PublicNewsService news) : PublicApiController
{
    /// <summary>Published news, newest first. Filter by category, tag, year or text.</summary>
    [HttpGet]
    public Task<PagedResult<NewsCardDto>> List(string projectSlug, [FromQuery] NewsQuery query, CancellationToken ct) =>
        news.ListAsync(projectSlug, query, ct);

    [HttpGet("categories")]
    public Task<IReadOnlyList<CategoryCountDto>> Categories(string projectSlug, CancellationToken ct) =>
        news.CategoriesAsync(projectSlug, ct);

    /// <summary>Article by slug. A renamed slug answers 301 to the canonical one.</summary>
    [HttpGet("{slug}")]
    [ProducesResponseType(StatusCodes.Status301MovedPermanently)]
    public Task<NewsDetailDto> Get(string projectSlug, string slug, CancellationToken ct) =>
        news.GetAsync(projectSlug, slug, ct);
}

[Route("api/v1/projects/{projectSlug}/stem-saturdays")]
public sealed class StemSaturdaysController(PublicEventService events) : PublicApiController
{
    /// <summary>Events: <c>scope=Upcoming</c> (soonest first), <c>Past</c> (archive, newest first) or <c>All</c>.</summary>
    [HttpGet]
    public Task<PagedResult<EventCardDto>> List(string projectSlug, [FromQuery] EventQuery query, CancellationToken ct) =>
        events.ListAsync(projectSlug, query, ct);

    /// <summary>Years, disciplines, tags, formats and types that occur in published events.</summary>
    [HttpGet("filters")]
    public Task<EventFiltersDto> Filters(string projectSlug, CancellationToken ct) => events.FiltersAsync(projectSlug, ct);

    /// <summary>Announcement and, after the event, its archive (summary, recording, photos, materials).</summary>
    [HttpGet("{slug}")]
    [ProducesResponseType(StatusCodes.Status301MovedPermanently)]
    public Task<EventDetailDto> Get(string projectSlug, string slug, CancellationToken ct) => events.GetAsync(projectSlug, slug, ct);
}

[Route("api/v1/projects/{projectSlug}")]
public sealed class PeopleController(PublicPeopleService people) : PublicApiController
{
    /// <summary>Team page: core team, consultants, technology team and organizational staff, current and former.</summary>
    [HttpGet("team")]
    public Task<TeamDto> Team(string projectSlug, CancellationToken ct) => people.TeamAsync(projectSlug, ct);

    [HttpGet("researchers")]
    public Task<PagedResult<PersonCardDto>> Researchers(string projectSlug, [FromQuery] PeopleQuery query, CancellationToken ct) =>
        people.ResearchersAsync(projectSlug, query, ct);

    [HttpGet("mentors")]
    public Task<PagedResult<PersonCardDto>> Mentors(string projectSlug, [FromQuery] PeopleQuery query, CancellationToken ct) =>
        people.MentorsAsync(projectSlug, query, ct);

    /// <summary>One profile for every role; includes the events the person took part in.</summary>
    [HttpGet("people/{slug}")]
    [ProducesResponseType(StatusCodes.Status301MovedPermanently)]
    public Task<PersonDetailDto> Person(string projectSlug, string slug, CancellationToken ct) => people.GetAsync(projectSlug, slug, ct);
}

[Route("api/v1/projects/{projectSlug}/resources")]
public sealed class ResourcesController(PublicResourceService resources) : PublicApiController
{
    [HttpGet]
    public Task<PagedResult<ResourceCardDto>> List(string projectSlug, [FromQuery] ResourceQuery query, CancellationToken ct) =>
        resources.ListAsync(projectSlug, query, ct);

    [HttpGet("{slug}")]
    [ProducesResponseType(StatusCodes.Status301MovedPermanently)]
    public Task<ResourceDetailDto> Get(string projectSlug, string slug, CancellationToken ct) => resources.GetAsync(projectSlug, slug, ct);
}

[Route("api/v1/projects/{projectSlug}/stem-ambassadors")]
public sealed class StemAmbassadorsController(PublicAmbassadorService ambassadors) : PublicApiController
{
    /// <summary>Program page plus the latest school activities.</summary>
    [HttpGet]
    public Task<AmbassadorsDto> Overview(string projectSlug, CancellationToken ct) => ambassadors.OverviewAsync(projectSlug, ct);

    [HttpGet("activities")]
    public Task<PagedResult<SchoolActivityCardDto>> Activities(string projectSlug, [FromQuery] PageQuery paging, CancellationToken ct) =>
        ambassadors.ListAsync(projectSlug, paging, ct);

    [HttpGet("activities/{slug}")]
    [ProducesResponseType(StatusCodes.Status301MovedPermanently)]
    public Task<SchoolActivityDetailDto> Activity(string projectSlug, string slug, CancellationToken ct) =>
        ambassadors.GetAsync(projectSlug, slug, ct);
}

/// <summary>Fixed page keys, exposed for the Angular router.</summary>
[Route("api/v1/page-keys")]
public sealed class PageKeysController : PublicApiController
{
    [HttpGet]
    public IReadOnlyList<string> Get() => PageKeys.All;
}
