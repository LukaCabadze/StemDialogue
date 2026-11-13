using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StemDialogue.Application.Admin;
using StemDialogue.Application.Common;
using StemDialogue.Application.Identity;
using StemDialogue.Application.Media;
using StemDialogue.Application.Organizations;
using StemDialogue.Application.People;
using StemDialogue.Application.Taxonomy;
using StemDialogue.Domain.Common;
using StemDialogue.Infrastructure.Identity;

namespace StemDialogue.Api.Controllers.Admin;

/// <summary>
/// People are shared across projects: one profile per person with any number of roles (spec §7).
/// </summary>
[Route("api/v1/admin/people")]
public sealed class AdminPeopleController(PeopleAdminService people) : AdminApiController
{
    [HttpGet]
    public Task<PagedResult<PersonAdminListItem>> List([FromQuery] PeopleAdminQuery query, CancellationToken ct) => people.ListAsync(query, ct);

    [HttpGet("{id:guid}")]
    public Task<PersonAdminDto> Get(Guid id, CancellationToken ct) => people.GetAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<PersonAdminDto>> Create(PersonUpsertRequest request, CancellationToken ct)
    {
        var created = await people.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public Task<PersonAdminDto> Update(Guid id, PersonUpsertRequest request, CancellationToken ct) => people.UpdateAsync(id, request, ct);

    /// <summary>Replaces only the roles: move between team sections, add researcher/mentor, or end a role.</summary>
    [HttpPut("{id:guid}/roles")]
    public Task<PersonAdminDto> UpdateRoles(Guid id, PersonRolesRequest request, CancellationToken ct) => people.UpdateRolesAsync(id, request, ct);

    /// <summary>Deletes an unused, unpublished profile. Former members should get a role end date instead.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await people.DeleteAsync(id, ct);
        return NoContent();
    }
}

/// <summary>Funders, universities and other organizations with official logos.</summary>
[Route("api/v1/admin/organizations")]
public sealed class AdminOrganizationsController(OrganizationAdminService organizations) : AdminApiController
{
    [HttpGet]
    public Task<IReadOnlyList<OrganizationAdminDto>> List(CancellationToken ct) => organizations.ListAsync(ct);

    [HttpGet("{id:guid}")]
    public Task<OrganizationAdminDto> Get(Guid id, CancellationToken ct) => organizations.GetAsync(id, ct);

    [HttpPost]
    [Authorize(Policy = AppPolicies.Administrator)]
    public async Task<ActionResult<OrganizationAdminDto>> Create(OrganizationUpsertRequest request, CancellationToken ct)
    {
        var created = await organizations.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AppPolicies.Administrator)]
    public Task<OrganizationAdminDto> Update(Guid id, OrganizationUpsertRequest request, CancellationToken ct) =>
        organizations.UpdateAsync(id, request, ct);

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AppPolicies.Administrator)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await organizations.DeleteAsync(id, ct);
        return NoContent();
    }
}

[Route("api/v1/admin")]
public sealed class AdminTaxonomyController(TaxonomyAdminService taxonomy) : AdminApiController
{
    [HttpGet("tags")]
    public Task<IReadOnlyList<TermAdminDto>> Tags(CancellationToken ct) => taxonomy.TagsAsync(ct);

    [HttpPost("tags")]
    public async Task<ActionResult<TermAdminDto>> CreateTag(TermUpsertRequest request, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await taxonomy.CreateTagAsync(request, ct));

    [HttpPut("tags/{id:guid}")]
    public Task<TermAdminDto> UpdateTag(Guid id, TermUpsertRequest request, CancellationToken ct) => taxonomy.UpdateTagAsync(id, request, ct);

    /// <summary>Deletes the tag and removes it from all content.</summary>
    [HttpDelete("tags/{id:guid}")]
    public async Task<IActionResult> DeleteTag(Guid id, CancellationToken ct)
    {
        await taxonomy.DeleteTagAsync(id, ct);
        return NoContent();
    }

    [HttpGet("disciplines")]
    public Task<IReadOnlyList<TermAdminDto>> Disciplines(CancellationToken ct) => taxonomy.DisciplinesAsync(ct);

    [HttpPost("disciplines")]
    public async Task<ActionResult<TermAdminDto>> CreateDiscipline(TermUpsertRequest request, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await taxonomy.CreateDisciplineAsync(request, ct));

    [HttpPut("disciplines/{id:guid}")]
    public Task<TermAdminDto> UpdateDiscipline(Guid id, TermUpsertRequest request, CancellationToken ct) =>
        taxonomy.UpdateDisciplineAsync(id, request, ct);

    [HttpDelete("disciplines/{id:guid}")]
    public async Task<IActionResult> DeleteDiscipline(Guid id, CancellationToken ct)
    {
        await taxonomy.DeleteDisciplineAsync(id, ct);
        return NoContent();
    }
}

/// <summary>Multipart upload form. Alt text is required for images used publicly.</summary>
public sealed class MediaUploadForm
{
    public IFormFile? File { get; set; }
    public string? AltKa { get; set; }
    public string? AltEn { get; set; }
    public string? CaptionKa { get; set; }
    public string? CaptionEn { get; set; }
    public string? CreditKa { get; set; }
    public string? CreditEn { get; set; }
}

[Route("api/v1/admin/media")]
public sealed class AdminMediaController(MediaService media) : AdminApiController
{
    private const long MaxRequestBytes = 60L * 1024 * 1024;

    [HttpGet]
    public Task<PagedResult<MediaAdminDto>> List([FromQuery] MediaAdminQuery query, CancellationToken ct) => media.ListAsync(query, ct);

    [HttpGet("{id:guid}")]
    public Task<MediaAdminDto> Get(Guid id, CancellationToken ct) => media.GetAsync(id, ct);

    /// <summary>Uploads an image (JPG/PNG/WebP/GIF/SVG, 10 MB) or document (PDF/PPTX/DOCX/XLSX, 50 MB). Content is verified on the server.</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    public async Task<ActionResult<MediaAdminDto>> Upload([FromForm] MediaUploadForm form, CancellationToken ct)
    {
        if (form.File is null) throw ContentReferences.Invalid("File", "Choose a file to upload.");

        await using var stream = form.File.OpenReadStream();
        var created = await media.UploadAsync(
            new MediaUpload(
                stream,
                form.File.FileName,
                form.File.Length,
                new LocalizedTextDto(form.AltKa, form.AltEn),
                new LocalizedTextDto(form.CaptionKa, form.CaptionEn),
                new LocalizedTextDto(form.CreditKa, form.CreditEn)),
            ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public Task<MediaAdminDto> UpdateMetadata(Guid id, MediaMetadataRequest request, CancellationToken ct) =>
        media.UpdateMetadataAsync(id, request, ct);

    /// <summary>Where the file is used (news, events, people, pages …).</summary>
    [HttpGet("{id:guid}/usages")]
    public Task<IReadOnlyList<MediaUsageDto>> Usages(Guid id, CancellationToken ct) => media.UsagesAsync(id, ct);

    /// <summary>Deletes an unused file; answers 409 with the usages otherwise.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await media.DeleteAsync(id, ct);
        return NoContent();
    }
}

/// <summary>Dashboard counters and dropdown values for CMS forms.</summary>
[Route("api/v1/admin/projects/{projectSlug}")]
public sealed class AdminOverviewController(AdminOverviewService overview) : AdminApiController
{
    [HttpGet("dashboard")]
    public Task<DashboardDto> Dashboard(string projectSlug, CancellationToken ct) => overview.DashboardAsync(projectSlug, ct);

    /// <summary>Enum labels and entity lists for selects. <c>?lang=en</c> for English labels.</summary>
    [HttpGet("lookups")]
    public Task<LookupsDto> Lookups(string projectSlug, [FromQuery] string? lang, CancellationToken ct) =>
        overview.LookupsAsync(projectSlug, lang == LocalizedText.English ? LocalizedText.English : LocalizedText.Georgian, ct);
}

/// <summary>Administrator-only: accounts and audit history.</summary>
[Route("api/v1/admin")]
[Authorize(Policy = AppPolicies.Administrator)]
public sealed class AdminUsersController(IUserAdminService users, AdminOverviewService overview) : AdminApiController
{
    [HttpGet("users")]
    public Task<IReadOnlyList<UserAdminDto>> List(CancellationToken ct) => users.ListAsync(ct);

    [HttpPost("users")]
    public async Task<ActionResult<UserAdminDto>> Create(UserCreateRequest request, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await users.CreateAsync(request, ct));

    /// <summary>Changes name, role or active state. Changes take effect immediately (sessions end).</summary>
    [HttpPut("users/{id}")]
    public Task<UserAdminDto> Update(string id, UserUpdateRequest request, CancellationToken ct) =>
        users.UpdateAsync(id, request, User.FindFirst(AppClaims.Subject)?.Value ?? string.Empty, ct);

    [HttpPost("users/{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(string id, PasswordResetRequest request, CancellationToken ct)
    {
        await users.ResetPasswordAsync(id, request, ct);
        return NoContent();
    }

    [HttpPost("users/{id}/unlock")]
    public async Task<IActionResult> Unlock(string id, CancellationToken ct)
    {
        await users.UnlockAsync(id, ct);
        return NoContent();
    }

    /// <summary>Change history of all content (who, what, when).</summary>
    [HttpGet("audit")]
    public Task<PagedResult<AuditEntryDto>> Audit([FromQuery] AuditQuery query, CancellationToken ct) => overview.AuditAsync(query, ct);
}
