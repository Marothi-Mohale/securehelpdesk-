namespace SecureHelpdesk.Application.DTOs.Common;

public class ApiErrorResponseDto
{
    public required int Status { get; init; }
    public required string Title { get; init; }
    public required string Detail { get; init; }
    public required string ErrorCode { get; init; }
    public required string TraceId { get; init; }
    public required string Path { get; init; }
    public required DateTime TimestampUtc { get; init; }
}
