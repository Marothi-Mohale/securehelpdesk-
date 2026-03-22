using System.ComponentModel.DataAnnotations;

namespace SecureHelpdesk.Application.DTOs.Tickets;

/// <summary>
/// Request payload for adding a discussion comment to a ticket.
/// </summary>
public class AddCommentRequestDto
{
    /// <summary>
    /// Human-readable comment content visible in the ticket timeline.
    /// </summary>
    [Required(ErrorMessage = "Comment content is required.")]
    [StringLength(1000, MinimumLength = 1, ErrorMessage = "Comment content must be between 1 and 1000 characters.")]
    public string Content { get; init; } = string.Empty;
}
