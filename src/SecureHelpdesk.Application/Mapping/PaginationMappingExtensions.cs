using SecureHelpdesk.Application.Common;
using SecureHelpdesk.Application.DTOs.Common;

namespace SecureHelpdesk.Application.Mapping;

public static class PaginationMappingExtensions
{
    public static PaginatedResponseDto<T> ToResponseDto<T>(this PagedResult<T> pagedResult)
    {
        return new PaginatedResponseDto<T>
        {
            Items = pagedResult.Items,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount
        };
    }
}
