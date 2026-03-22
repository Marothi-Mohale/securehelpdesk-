using System.ComponentModel.DataAnnotations;

namespace SecureHelpdesk.Application.DTOs.Tickets;

public class AddCommentRequestDto
{
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Content { get; init; } = string.Empty;
}
