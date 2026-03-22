using System.ComponentModel.DataAnnotations;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.DTOs.Tickets;

/// <summary>
/// Request payload for creating a new helpdesk ticket.
/// </summary>
public class CreateTicketRequestDto
{
    /// <summary>
    /// Short summary of the issue.
    /// </summary>
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters.")]
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Detailed problem description for agents and auditors.
    /// </summary>
    [Required(ErrorMessage = "Description is required.")]
    [StringLength(4000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 4000 characters.")]
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Business impact or urgency level selected by the requester.
    /// </summary>
    [EnumDataType(typeof(TicketPriority), ErrorMessage = "Priority must be a valid ticket priority.")]
    public TicketPriority Priority { get; init; } = TicketPriority.Medium;
}
