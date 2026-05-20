namespace Crestacle.Bedrock.Core.DTOs;

/// <summary>Wraps a page of items with total-count metadata for paginated API responses.</summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}
