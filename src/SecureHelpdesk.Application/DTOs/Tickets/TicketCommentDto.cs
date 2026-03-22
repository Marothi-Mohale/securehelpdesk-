namespace SecureHelpdesk.Application.DTOs.Tickets;

public class TicketCommentDto
{
    public required Guid Id { get; init; }
    public required string Content { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required string UserId { get; init; }
    public required string UserName { get; init; }
}
