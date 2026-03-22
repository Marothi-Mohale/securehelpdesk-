using System.Diagnostics;
using System.Security.Claims;

namespace SecureHelpdesk.Api.Middleware;

public class RequestLoggingMiddleware
{
    private static readonly PathString SwaggerPath = new("/swagger");

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers["X-Trace-Id"] = context.TraceIdentifier;

        if (context.Request.Path.StartsWithSegments(SwaggerPath))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        using (_logger.BeginScope(new Dictionary<string, object?>
        {
            ["TraceId"] = context.TraceIdentifier,
            ["RequestMethod"] = context.Request.Method,
            ["RequestPath"] = context.Request.Path.Value,
            ["UserId"] = userId
        }))
        {
            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                LogCompletedRequest(context, stopwatch.Elapsed.TotalMilliseconds, userId);
            }
        }
    }

    private void LogCompletedRequest(HttpContext context, double elapsedMilliseconds, string? userId)
    {
        var statusCode = context.Response.StatusCode;
        var elapsed = Math.Round(elapsedMilliseconds, 2);

        if (statusCode >= 500)
        {
            _logger.LogError(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
                context.Request.Method,
                context.Request.Path.Value,
                statusCode,
                elapsed);
            return;
        }

        if (statusCode >= 400)
        {
            _logger.LogWarning(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms for user {UserId}",
                context.Request.Method,
                context.Request.Path.Value,
                statusCode,
                elapsed,
                userId ?? "anonymous");
            return;
        }

        _logger.LogInformation(
            "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
            context.Request.Method,
            context.Request.Path.Value,
            statusCode,
            elapsed);
    }
}
