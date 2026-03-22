using System.ComponentModel.DataAnnotations;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.DTOs.Tickets;

public class UpdateTicketStatusRequestDto
{
    [EnumDataType(typeof(TicketStatus))]
    public TicketStatus Status { get; init; }
}
