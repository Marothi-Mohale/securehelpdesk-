using Microsoft.AspNetCore.Http;
using SecureHelpdesk.Application.DTOs.Common;

namespace SecureHelpdesk.Application.Common;

public static class ApiErrorResponseFactory
{
    public static ApiErrorResponseDto Create(
        HttpContext context,
        int statusCode,
        string detail,
        string? errorCode = null,
        string? title = null)
    {
        return new ApiErrorResponseDto
        {
            Status = statusCode,
            Title = title ?? GetDefaultTitle(statusCode),
            Detail = detail,
            ErrorCode = errorCode ?? GetDefaultErrorCode(statusCode),
            TraceId = context.TraceIdentifier,
            Path = context.Request.Path.Value ?? string.Empty,
            TimestampUtc = DateTime.UtcNow
        };
    }

    public static ValidationErrorResponseDto CreateValidation(
        HttpContext context,
        IReadOnlyDictionary<string, string[]> errors,
        string detail = "One or more validation errors occurred.")
    {
        return new ValidationErrorResponseDto
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Failed",
            Detail = detail,
            ErrorCode = "validation_failed",
            TraceId = context.TraceIdentifier,
            Path = context.Request.Path.Value ?? string.Empty,
            TimestampUtc = DateTime.UtcNow,
            Errors = errors
        };
    }

    private static string GetDefaultTitle(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            _ when statusCode >= 500 => "Server Error",
            _ => "Request Failed"
        };
    }

    private static string GetDefaultErrorCode(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "bad_request",
            StatusCodes.Status401Unauthorized => "unauthorized",
            StatusCodes.Status403Forbidden => "forbidden",
            StatusCodes.Status404NotFound => "not_found",
            StatusCodes.Status409Conflict => "conflict",
            _ when statusCode >= 500 => "server_error",
            _ => "request_failed"
        };
    }
}
