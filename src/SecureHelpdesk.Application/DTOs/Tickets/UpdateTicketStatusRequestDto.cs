using System.ComponentModel.DataAnnotations;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.DTOs.Tickets;

public class UpdateTicketStatusRequestDto
{
    [EnumDataType(typeof(TicketStatus), ErrorMessage = "Status must be a valid ticket status.")]
    public TicketStatus Status { get; init; }
}
