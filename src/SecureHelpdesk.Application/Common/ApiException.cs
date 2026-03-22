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
}
