using System.ComponentModel.DataAnnotations;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.DTOs.Tickets;

public class UpdateTicketRequestDto
{
    [StringLength(200, MinimumLength = 5)]
    public string? Title { get; init; }

    [StringLength(4000, MinimumLength = 10)]
    public string? Description { get; init; }

    [EnumDataType(typeof(TicketPriority))]
    public TicketPriority? Priority { get; init; }
}
