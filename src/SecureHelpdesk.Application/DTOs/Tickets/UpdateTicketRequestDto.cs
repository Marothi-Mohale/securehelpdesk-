using System.ComponentModel.DataAnnotations;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.DTOs.Tickets;

/// <summary>
/// Partial update payload for editable ticket fields.
/// </summary>
public class UpdateTicketRequestDto
{
    /// <summary>
    /// Updated ticket title when the original summary needs refinement.
    /// </summary>
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters.")]
    public string? Title { get; init; }

    /// <summary>
    /// Updated ticket description with additional operational detail.
    /// </summary>
    [StringLength(4000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 4000 characters.")]
    public string? Description { get; init; }

    /// <summary>
    /// Updated priority when business impact changes.
    /// </summary>
    [EnumDataType(typeof(TicketPriority), ErrorMessage = "Priority must be a valid ticket priority.")]
    public TicketPriority? Priority { get; init; }
}
