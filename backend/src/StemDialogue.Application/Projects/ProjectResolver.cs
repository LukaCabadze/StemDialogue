using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StemDialogue.Application.Abstractions;
using StemDialogue.Application.Common;

namespace StemDialogue.Application.Projects;

public sealed record ProjectRef(Guid Id, string Slug);

/// <summary>Resolves the <c>{projectSlug}</c> route segment. Project slugs never change, so results are cached.</summary>
public sealed class ProjectResolver(IAppDbContext db, IMemoryCache cache)
{
    public async Task<ProjectRef> ResolveAsync(string slug, CancellationToken ct)
    {
        var normalized = slug.Trim().ToLowerInvariant();
        var key = "project:" + normalized;
        if (cache.TryGetValue(key, out ProjectRef? cached) && cached is not null) return cached;

        var project = await db.Projects
            .AsNoTracking()
            .Where(p => p.Slug == normalized)
            .Select(p => new ProjectRef(p.Id, p.Slug))
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("Project", slug);

        cache.Set(key, project, TimeSpan.FromMinutes(30));
        return project;
    }
}
