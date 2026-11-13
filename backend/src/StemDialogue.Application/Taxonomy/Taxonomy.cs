using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StemDialogue.Application.Abstractions;
using StemDialogue.Application.Common;
using StemDialogue.Domain.Taxonomy;

namespace StemDialogue.Application.Taxonomy;

public sealed record TermUpsertRequest(LocalizedTextDto Name, string? Slug, int DisplayOrder = 0);

public sealed record TermAdminDto(Guid Id, string Slug, LocalizedTextDto Name, int DisplayOrder, int UsageCount);

public sealed class TermUpsertValidator : AbstractValidator<TermUpsertRequest>
{
    public TermUpsertValidator()
    {
        RuleFor(x => x.Name).RequiredText(80);
        RuleFor(x => x.Slug).OptionalSlug();
        RuleFor(x => x.DisplayOrder).InclusiveBetween(0, 1000);
    }
}

/// <summary>Tags and STEM disciplines used by filters.</summary>
public sealed class TaxonomyAdminService(IAppDbContext db)
{
    public async Task<IReadOnlyList<TermAdminDto>> TagsAsync(CancellationToken ct) =>
        await db.Tags.AsNoTracking()
            .OrderBy(t => t.Name.Ka)
            .Select(t => new TermAdminDto(
                t.Id,
                t.Slug,
                new LocalizedTextDto(t.Name.Ka, t.Name.En),
                0,
                db.News.Count(n => n.Tags.Any(x => x.Id == t.Id))
                + db.Events.Count(e => e.Tags.Any(x => x.Id == t.Id))
                + db.Resources.Count(r => r.Tags.Any(x => x.Id == t.Id))))
            .ToListAsync(ct);

    public async Task<TermAdminDto> CreateTagAsync(TermUpsertRequest request, CancellationToken ct)
    {
        var tag = new Tag();
        tag.Name.Apply(request.Name);
        tag.Slug = await UniqueTagSlugAsync(request, tag.Id, ct);
        db.Tags.Add(tag);
        await db.SaveChangesAsync(ct);
        return new TermAdminDto(tag.Id, tag.Slug, tag.Name.ToDto(), 0, 0);
    }

    public async Task<TermAdminDto> UpdateTagAsync(Guid id, TermUpsertRequest request, CancellationToken ct)
    {
        var tag = await db.Tags.FirstOrDefaultAsync(t => t.Id == id, ct) ?? throw new NotFoundException("Tag", id);
        tag.Name.Apply(request.Name);
        tag.Slug = await UniqueTagSlugAsync(request, tag.Id, ct);
        await db.SaveChangesAsync(ct);
        return (await TagsAsync(ct)).First(t => t.Id == id);
    }

    /// <summary>Removes the tag from all content.</summary>
    public async Task DeleteTagAsync(Guid id, CancellationToken ct)
    {
        var tag = await db.Tags.FirstOrDefaultAsync(t => t.Id == id, ct) ?? throw new NotFoundException("Tag", id);
        db.Tags.Remove(tag);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<TermAdminDto>> DisciplinesAsync(CancellationToken ct) =>
        await db.Disciplines.AsNoTracking()
            .OrderBy(d => d.DisplayOrder).ThenBy(d => d.Name.Ka)
            .Select(d => new TermAdminDto(
                d.Id,
                d.Slug,
                new LocalizedTextDto(d.Name.Ka, d.Name.En),
                d.DisplayOrder,
                db.Events.Count(e => e.Disciplines.Any(x => x.Id == d.Id))
                + db.People.Count(p => p.Disciplines.Any(x => x.Id == d.Id))
                + db.Resources.Count(r => r.Disciplines.Any(x => x.Id == d.Id))))
            .ToListAsync(ct);

    public async Task<TermAdminDto> CreateDisciplineAsync(TermUpsertRequest request, CancellationToken ct)
    {
        var discipline = new Discipline { DisplayOrder = request.DisplayOrder };
        discipline.Name.Apply(request.Name);
        discipline.Slug = await UniqueDisciplineSlugAsync(request, discipline.Id, ct);
        db.Disciplines.Add(discipline);
        await db.SaveChangesAsync(ct);
        return new TermAdminDto(discipline.Id, discipline.Slug, discipline.Name.ToDto(), discipline.DisplayOrder, 0);
    }

    public async Task<TermAdminDto> UpdateDisciplineAsync(Guid id, TermUpsertRequest request, CancellationToken ct)
    {
        var discipline = await db.Disciplines.FirstOrDefaultAsync(d => d.Id == id, ct) ?? throw new NotFoundException("Discipline", id);
        discipline.Name.Apply(request.Name);
        discipline.DisplayOrder = request.DisplayOrder;
        discipline.Slug = await UniqueDisciplineSlugAsync(request, discipline.Id, ct);
        await db.SaveChangesAsync(ct);
        return (await DisciplinesAsync(ct)).First(d => d.Id == id);
    }

    public async Task DeleteDisciplineAsync(Guid id, CancellationToken ct)
    {
        var discipline = await db.Disciplines.FirstOrDefaultAsync(d => d.Id == id, ct) ?? throw new NotFoundException("Discipline", id);
        db.Disciplines.Remove(discipline);
        await db.SaveChangesAsync(ct);
    }

    private Task<string> UniqueTagSlugAsync(TermUpsertRequest request, Guid id, CancellationToken ct) =>
        Slugifier.EnsureUniqueAsync(
            Slugifier.From(string.IsNullOrWhiteSpace(request.Slug) ? request.Name.Ka : request.Slug),
            candidate => db.Tags.AnyAsync(t => t.Slug == candidate && t.Id != id, ct));

    private Task<string> UniqueDisciplineSlugAsync(TermUpsertRequest request, Guid id, CancellationToken ct) =>
        Slugifier.EnsureUniqueAsync(
            Slugifier.From(string.IsNullOrWhiteSpace(request.Slug) ? request.Name.Ka : request.Slug),
            candidate => db.Disciplines.AnyAsync(d => d.Slug == candidate && d.Id != id, ct));
}
