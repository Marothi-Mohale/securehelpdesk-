using System.ComponentModel.DataAnnotations;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.DTOs.Tickets;

public class CreateTicketRequestDto
{
    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; init; } = string.Empty;

    [Required]
    [StringLength(4000, MinimumLength = 10)]
    public string Description { get; init; } = string.Empty;

    [EnumDataType(typeof(TicketPriority))]
    public TicketPriority Priority { get; init; } = TicketPriority.Medium;
}
