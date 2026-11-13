using Microsoft.EntityFrameworkCore;

namespace StemDialogue.Application.Common;

/// <summary>Query-string paging parameters. Page size is clamped to 1–100.</summary>
public class PageQuery
{
    public const int MaxPageSize = 100;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;

    public int SafePage => Math.Max(1, Page);
    public int SafePageSize => Math.Clamp(PageSize, 1, MaxPageSize);
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(Total / (double)PageSize);
    public bool HasMore => Page < TotalPages;

    public PagedResult<TOut> Map<TOut>(Func<T, TOut> map) => new(Items.Select(map).ToList(), Total, Page, PageSize);
}

public static class PagingExtensions
{
    public static async Task<PagedResult<T>> ToPagedAsync<T>(this IQueryable<T> query, PageQuery paging, CancellationToken ct)
    {
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((paging.SafePage - 1) * paging.SafePageSize)
            .Take(paging.SafePageSize)
            .ToListAsync(ct);
        return new PagedResult<T>(items, total, paging.SafePage, paging.SafePageSize);
    }
}
