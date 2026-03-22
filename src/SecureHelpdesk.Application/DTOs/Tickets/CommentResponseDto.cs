namespace SecureHelpdesk.Application.DTOs.Tickets;

public class CommentResponseDto
{
    public required Guid Id { get; init; }
    public required string Content { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required string AuthorUserId { get; init; }
    public required string AuthorName { get; init; }
}
