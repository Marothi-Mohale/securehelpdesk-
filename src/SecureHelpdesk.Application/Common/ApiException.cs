using Microsoft.AspNetCore.Http;

namespace SecureHelpdesk.Application.Common;

public class ApiException : Exception
{
    public ApiException(
        string message,
        int statusCode,
        string? errorCode = null,
        string? title = null,
        IReadOnlyDictionary<string, string[]>? errors = null) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Title = title;
        Errors = errors;
    }

    public int StatusCode { get; }
    public string? ErrorCode { get; }
    public string? Title { get; }
    public IReadOnlyDictionary<string, string[]>? Errors { get; }

    public static ApiException Validation(
        string detail,
        IReadOnlyDictionary<string, string[]>? errors = null)
    {
        return new ApiException(
            detail,
            StatusCodes.Status400BadRequest,
            ErrorCodes.ValidationFailed,
            "Validation Failed",
            errors);
    }

    public static ApiException BadRequest(string detail, string? errorCode = null, string? title = null)
    {
        return new ApiException(
            detail,
            StatusCodes.Status400BadRequest,
            errorCode ?? ErrorCodes.BadRequest,
            title);
    }

    public static ApiException Unauthorized(string detail, string? errorCode = null)
    {
        return new ApiException(
            detail,
            StatusCodes.Status401Unauthorized,
            errorCode ?? ErrorCodes.Unauthorized);
    }

    public static ApiException Forbidden(string detail, string? errorCode = null)
    {
        return new ApiException(
            detail,
            StatusCodes.Status403Forbidden,
            errorCode ?? ErrorCodes.Forbidden);
    }

    public static ApiException NotFound(string detail, string? errorCode = null)
    {
        return new ApiException(
            detail,
            StatusCodes.Status404NotFound,
            errorCode ?? ErrorCodes.NotFound);
    }

    public static ApiException Conflict(string detail, string? errorCode = null)
    {
        return new ApiException(
            detail,
            StatusCodes.Status409Conflict,
            errorCode ?? ErrorCodes.Conflict);
    }
}
