using System.ComponentModel.DataAnnotations;

namespace SecureHelpdesk.Application.DTOs.Tickets;

public class AssignTicketRequestDto
{
    [Required]
    public string AgentUserId { get; init; } = string.Empty;
}
