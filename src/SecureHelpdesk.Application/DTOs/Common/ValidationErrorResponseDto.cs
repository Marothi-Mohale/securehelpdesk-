namespace SecureHelpdesk.Application.DTOs.Common;

public class ValidationErrorResponseDto : ApiErrorResponseDto
{
    public required IReadOnlyDictionary<string, string[]> Errors { get; init; }
}
