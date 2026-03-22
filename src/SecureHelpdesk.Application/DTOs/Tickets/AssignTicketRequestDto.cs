using System.ComponentModel.DataAnnotations;

namespace SecureHelpdesk.Application.DTOs.Tickets;

/// <summary>
/// Request payload for assigning a ticket to an agent.
/// </summary>
public class AssignTicketRequestDto
{
    /// <summary>
    /// Identity user id of the target agent.
    /// </summary>
    [Required(ErrorMessage = "Agent user id is required.")]
    [StringLength(450, MinimumLength = 1, ErrorMessage = "Agent user id must be between 1 and 450 characters.")]
    public string AgentUserId { get; init; } = string.Empty;
}
