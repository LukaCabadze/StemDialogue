using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using StemDialogue.Api.Web;
using StemDialogue.Application.Ambassadors;
using StemDialogue.Application.Common;
using StemDialogue.Application.Events;
using StemDialogue.Application.Identity;
using StemDialogue.Application.News;
using StemDialogue.Application.Pages;
using StemDialogue.Application.Projects;
using StemDialogue.Application.Resources;

namespace StemDialogue.Api.Controllers.Admin;

/// <summary>Base for CMS endpoints: Admin or Editor, never cached, public cache evicted after changes.</summary>
[ApiController]
[Authorize(Policy = AppPolicies.ContentEditor)]
[ServiceFilter(typeof(PublicCacheEvictionFilter))]
[Produces("application/json")]
public abstract class AdminApiController : ControllerBase;

/// <summary>Project settings: names, funding wording for header/footer, partners, contacts, social links.</summary>
[Route("api/v1/admin/projects")]
public sealed class AdminProjectsController(ProjectAdminService projects) : AdminApiController
{
    [HttpGet]
    public Task<IReadOnlyList<ProjectSummaryDto>> List(CancellationToken ct) => projects.ListAsync(ct);

    [HttpGet("{projectSlug}")]
    public Task<ProjectAdminDto> Get(string projectSlug, CancellationToken ct) => projects.GetAsync(projectSlug, ct);

    /// <summary>Funding and partner settings are Admin-only.</summary>
    [HttpPut("{projectSlug}")]
    [Authorize(Policy = AppPolicies.Administrator)]
    public Task<ProjectAdminDto> Update(string projectSlug, ProjectUpdateRequest request, CancellationToken ct) =>
        projects.UpdateAsync(projectSlug, request, ct);
}

[Route("api/v1/admin/projects/{projectSlug}/pages")]
public sealed class AdminPagesController(PageAdminService pages) : AdminApiController
{
    [HttpGet]
    public Task<IReadOnlyList<PageAdminListItem>> List(string projectSlug, CancellationToken ct) => pages.ListAsync(projectSlug, ct);

    [HttpGet("{key}")]
    public Task<PageAdminDto> Get(string projectSlug, string key, CancellationToken ct) => pages.GetAsync(projectSlug, key, ct);

    [HttpPut("{key}")]
    public Task<PageAdminDto> Update(string projectSlug, string key, PageUpdateRequest request, CancellationToken ct) =>
        pages.UpdateAsync(projectSlug, key, request, ct);
}

[Route("api/v1/admin/projects/{projectSlug}/milestones")]
public sealed class AdminMilestonesController(MilestoneAdminService milestones) : AdminApiController
{
    [HttpGet]
    public Task<IReadOnlyList<MilestoneAdminDto>> List(string projectSlug, CancellationToken ct) => milestones.ListAsync(projectSlug, ct);

    [HttpPost]
    public async Task<ActionResult<MilestoneAdminDto>> Create(string projectSlug, MilestoneUpsertRequest request, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await milestones.CreateAsync(projectSlug, request, ct));

    [HttpPut("{id:guid}")]
    public Task<MilestoneAdminDto> Update(string projectSlug, Guid id, MilestoneUpsertRequest request, CancellationToken ct) =>
        milestones.UpdateAsync(projectSlug, id, request, ct);

    /// <summary>Sets the display order; send every milestone id once.</summary>
    [HttpPut("order")]
    public Task<IReadOnlyList<MilestoneAdminDto>> Reorder(string projectSlug, ReorderRequest request, CancellationToken ct) =>
        milestones.ReorderAsync(projectSlug, request, ct);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(string projectSlug, Guid id, CancellationToken ct)
    {
        await milestones.DeleteAsync(projectSlug, id, ct);
        return NoContent();
    }
}

/// <summary>News and weekly announcements (spec §5).</summary>
[Route("api/v1/admin/projects/{projectSlug}/news")]
public sealed class AdminNewsController(NewsAdminService news) : AdminApiController
{
    [HttpGet]
    public Task<PagedResult<NewsAdminListItem>> List(string projectSlug, [FromQuery] NewsAdminQuery query, CancellationToken ct) =>
        news.ListAsync(projectSlug, query, ct);

    [HttpGet("{id:guid}")]
    public Task<NewsAdminDto> Get(string projectSlug, Guid id, CancellationToken ct) => news.GetAsync(projectSlug, id, ct);

    /// <summary>Creates a draft.</summary>
    [HttpPost]
    public async Task<ActionResult<NewsAdminDto>> Create(string projectSlug, NewsUpsertRequest request, CancellationToken ct)
    {
        var created = await news.CreateAsync(projectSlug, request, ct);
        return CreatedAtAction(nameof(Get), new { projectSlug, id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public Task<NewsAdminDto> Update(string projectSlug, Guid id, NewsUpsertRequest request, CancellationToken ct) =>
        news.UpdateAsync(projectSlug, id, request, ct);

    /// <summary>Publishes now, or schedules with a future <c>publishAtUtc</c>.</summary>
    [HttpPost("{id:guid}/publish")]
    public Task<NewsAdminDto> Publish(
        string projectSlug,
        Guid id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] PublishRequest? request,
        CancellationToken ct) =>
        news.PublishAsync(projectSlug, id, request ?? new PublishRequest(null), ct);

    [HttpPost("{id:guid}/unpublish")]
    public Task<NewsAdminDto> Unpublish(string projectSlug, Guid id, CancellationToken ct) => news.UnpublishAsync(projectSlug, id, ct);

    [HttpPost("{id:guid}/archive")]
    public Task<NewsAdminDto> Archive(string projectSlug, Guid id, CancellationToken ct) => news.ArchiveAsync(projectSlug, id, ct);

    /// <summary>Deletes a never-published draft.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(string projectSlug, Guid id, CancellationToken ct)
    {
        await news.DeleteAsync(projectSlug, id, ct);
        return NoContent();
    }
}

/// <summary>STEM Saturdays and other events, with their lifecycle (spec §6).</summary>
[Route("api/v1/admin/projects/{projectSlug}/events")]
public sealed class AdminEventsController(EventAdminService events) : AdminApiController
{
    [HttpGet]
    public Task<PagedResult<EventAdminListItem>> List(string projectSlug, [FromQuery] EventAdminQuery query, CancellationToken ct) =>
        events.ListAsync(projectSlug, query, ct);

    [HttpGet("{id:guid}")]
    public Task<EventAdminDto> Get(string projectSlug, Guid id, CancellationToken ct) => events.GetAsync(projectSlug, id, ct);

    /// <summary>Creates a planned event as a draft.</summary>
    [HttpPost]
    public async Task<ActionResult<EventAdminDto>> Create(string projectSlug, EventUpsertRequest request, CancellationToken ct)
    {
        var created = await events.CreateAsync(projectSlug, request, ct);
        return CreatedAtAction(nameof(Get), new { projectSlug, id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public Task<EventAdminDto> Update(string projectSlug, Guid id, EventUpsertRequest request, CancellationToken ct) =>
        events.UpdateAsync(projectSlug, id, request, ct);

    [HttpPost("{id:guid}/publish")]
    public Task<EventAdminDto> Publish(string projectSlug, Guid id, CancellationToken ct) => events.PublishAsync(projectSlug, id, ct);

    [HttpPost("{id:guid}/unpublish")]
    public Task<EventAdminDto> Unpublish(string projectSlug, Guid id, CancellationToken ct) => events.UnpublishAsync(projectSlug, id, ct);

    /// <summary>Marks the event as held; it moves to the archive at the same URL.</summary>
    [HttpPost("{id:guid}/complete")]
    public Task<EventAdminDto> Complete(
        string projectSlug,
        Guid id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CompleteEventRequest? request,
        CancellationToken ct) =>
        events.CompleteAsync(projectSlug, id, request ?? new CompleteEventRequest(null, null), ct);

    /// <summary>Cancels with a public note. The page and URL stay available.</summary>
    [HttpPost("{id:guid}/cancel")]
    public Task<EventAdminDto> Cancel(string projectSlug, Guid id, CancelEventRequest request, CancellationToken ct) =>
        events.CancelAsync(projectSlug, id, request, ct);

    /// <summary>Moves the event to a new date; the original date is kept for display.</summary>
    [HttpPost("{id:guid}/postpone")]
    public Task<EventAdminDto> Postpone(string projectSlug, Guid id, PostponeEventRequest request, CancellationToken ct) =>
        events.PostponeAsync(projectSlug, id, request, ct);

    /// <summary>Reverts a cancellation.</summary>
    [HttpPost("{id:guid}/restore")]
    public Task<EventAdminDto> Restore(string projectSlug, Guid id, CancellationToken ct) => events.RestoreAsync(projectSlug, id, ct);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(string projectSlug, Guid id, CancellationToken ct)
    {
        await events.DeleteAsync(projectSlug, id, ct);
        return NoContent();
    }
}

/// <summary>Videos, presentations, documents and links (spec §9).</summary>
[Route("api/v1/admin/projects/{projectSlug}/resources")]
public sealed class AdminResourcesController(ResourceAdminService resources) : AdminApiController
{
    [HttpGet]
    public Task<PagedResult<ResourceAdminListItem>> List(string projectSlug, [FromQuery] ResourceAdminQuery query, CancellationToken ct) =>
        resources.ListAsync(projectSlug, query, ct);

    [HttpGet("{id:guid}")]
    public Task<ResourceAdminDto> Get(string projectSlug, Guid id, CancellationToken ct) => resources.GetAsync(projectSlug, id, ct);

    [HttpPost]
    public async Task<ActionResult<ResourceAdminDto>> Create(string projectSlug, ResourceUpsertRequest request, CancellationToken ct)
    {
        var created = await resources.CreateAsync(projectSlug, request, ct);
        return CreatedAtAction(nameof(Get), new { projectSlug, id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public Task<ResourceAdminDto> Update(string projectSlug, Guid id, ResourceUpsertRequest request, CancellationToken ct) =>
        resources.UpdateAsync(projectSlug, id, request, ct);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(string projectSlug, Guid id, CancellationToken ct)
    {
        await resources.DeleteAsync(projectSlug, id, ct);
        return NoContent();
    }
}

/// <summary>STEM ambassador school activities (spec §8).</summary>
[Route("api/v1/admin/projects/{projectSlug}/school-activities")]
public sealed class AdminSchoolActivitiesController(SchoolActivityAdminService activities) : AdminApiController
{
    [HttpGet]
    public Task<PagedResult<SchoolActivityAdminListItem>> List(string projectSlug, [FromQuery] SchoolActivityAdminQuery query, CancellationToken ct) =>
        activities.ListAsync(projectSlug, query, ct);

    [HttpGet("{id:guid}")]
    public Task<SchoolActivityAdminDto> Get(string projectSlug, Guid id, CancellationToken ct) => activities.GetAsync(projectSlug, id, ct);

    [HttpPost]
    public async Task<ActionResult<SchoolActivityAdminDto>> Create(string projectSlug, SchoolActivityUpsertRequest request, CancellationToken ct)
    {
        var created = await activities.CreateAsync(projectSlug, request, ct);
        return CreatedAtAction(nameof(Get), new { projectSlug, id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public Task<SchoolActivityAdminDto> Update(string projectSlug, Guid id, SchoolActivityUpsertRequest request, CancellationToken ct) =>
        activities.UpdateAsync(projectSlug, id, request, ct);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(string projectSlug, Guid id, CancellationToken ct)
    {
        await activities.DeleteAsync(projectSlug, id, ct);
        return NoContent();
    }
}
