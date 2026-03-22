using System.ComponentModel.DataAnnotations;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.DTOs.Tickets;

public class CreateTicketRequestDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters.")]
    public string Title { get; init; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(4000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 4000 characters.")]
    public string Description { get; init; } = string.Empty;

    [EnumDataType(typeof(TicketPriority), ErrorMessage = "Priority must be a valid ticket priority.")]
    public TicketPriority Priority { get; init; } = TicketPriority.Medium;
}
