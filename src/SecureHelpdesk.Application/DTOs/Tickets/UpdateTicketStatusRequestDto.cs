using System.ComponentModel.DataAnnotations;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.DTOs.Tickets;

/// <summary>
/// Request payload for moving a ticket through the helpdesk workflow.
/// </summary>
public class UpdateTicketStatusRequestDto
{
    /// <summary>
    /// Target status for the ticket. Valid transitions are enforced server-side.
    /// </summary>
    [EnumDataType(typeof(TicketStatus), ErrorMessage = "Status must be a valid ticket status.")]
    public TicketStatus Status { get; init; }
}
