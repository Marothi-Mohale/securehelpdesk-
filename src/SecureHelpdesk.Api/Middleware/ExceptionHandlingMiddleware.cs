using SecureHelpdesk.Application.Common;

namespace SecureHelpdesk.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException exception)
        {
            _logger.LogWarning(
                exception,
                "Handled application exception with status {StatusCode} and error code {ErrorCode}",
                exception.StatusCode,
                exception.ErrorCode ?? "n/a");
            await WriteErrorResponseAsync(
                context,
                exception.StatusCode,
                exception.Message,
                exception.ErrorCode,
                exception.Title,
                exception.Errors);
        }
        catch (UnauthorizedAccessException exception)
        {
            _logger.LogWarning(exception, "Unauthorized access exception");
            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "Authentication is required to access this resource.");
        }
        catch (BadHttpRequestException exception)
        {
            _logger.LogWarning(exception, "Bad HTTP request");
            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status400BadRequest,
                exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception while processing request");
            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "An unexpected server error occurred.");
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        int statusCode,
        string detail,
        string? errorCode = null,
        string? title = null,
        IReadOnlyDictionary<string, string[]>? errors = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        object payload = errors is not null
            ? ApiErrorResponseFactory.CreateValidation(context, errors, detail)
            : ApiErrorResponseFactory.Create(context, statusCode, detail, errorCode, title);

        await context.Response.WriteAsJsonAsync(payload);
    }
}
