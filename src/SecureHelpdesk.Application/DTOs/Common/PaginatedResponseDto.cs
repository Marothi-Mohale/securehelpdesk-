namespace SecureHelpdesk.Application.DTOs.Common;

/// <summary>
/// Standard wrapper for paginated list responses.
/// </summary>
public class PaginatedResponseDto<T>
{
    public required IReadOnlyCollection<T> Items { get; init; }
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
