using SecureHelpdesk.Application.Common;
using SecureHelpdesk.Domain.Enums;

namespace SecureHelpdesk.Application.Models;

public static class TicketWorkflowRules
{
    public static void EnsureValidStatusTransition(TicketStatus currentStatus, TicketStatus newStatus)
    {
        if (GetAllowedNextStatuses(currentStatus).Contains(newStatus))
        {
            return;
        }

        throw ApiException.BadRequest(
            $"Status transition from {currentStatus} to {newStatus} is not allowed.",
            ErrorCodes.TicketInvalidStatusTransition);
    }

    public static IReadOnlyCollection<TicketStatus> GetAllowedNextStatuses(TicketStatus currentStatus)
    {
        return currentStatus switch
        {
            TicketStatus.Open => [TicketStatus.InProgress, TicketStatus.Closed],
            TicketStatus.InProgress => [TicketStatus.Resolved, TicketStatus.Closed],
            TicketStatus.Resolved => [TicketStatus.Closed, TicketStatus.InProgress],
            TicketStatus.Closed => [TicketStatus.InProgress],
            _ => []
        };
    }
}
