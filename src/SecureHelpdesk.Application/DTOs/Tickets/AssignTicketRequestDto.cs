using System.ComponentModel.DataAnnotations;

namespace SecureHelpdesk.Application.DTOs.Tickets;

public class AssignTicketRequestDto
{
    [Required(ErrorMessage = "Agent user id is required.")]
    public string AgentUserId { get; init; } = string.Empty;
}
