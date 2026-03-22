using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
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
            _logger.LogWarning(exception, "Handled application exception");
            await WriteProblemDetailsAsync(context, exception.StatusCode, exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception");
            await WriteProblemDetailsAsync(context, StatusCodes.Status500InternalServerError, "An unexpected server error occurred.");
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, int statusCode, string detail)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var payload = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode >= 500 ? "Server error" : "Request failed",
            Detail = detail
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
