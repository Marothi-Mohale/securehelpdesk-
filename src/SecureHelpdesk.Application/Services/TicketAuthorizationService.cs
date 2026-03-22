using SecureHelpdesk.Application.Common;
using SecureHelpdesk.Application.Interfaces;
using SecureHelpdesk.Domain.Entities;

namespace SecureHelpdesk.Application.Services;

public class TicketAuthorizationService : ITicketAuthorizationService
{
    public IQueryable<Ticket> ApplyListScope(IQueryable<Ticket> query, UserContext userContext)
    {
        if (userContext.IsAdmin)
        {
            return query;
        }

        if (userContext.IsAgent)
        {
            return query.Where(ticket =>
                ticket.AssignedToUserId == userContext.UserId ||
                ticket.CreatedByUserId == userContext.UserId);
        }

        if (userContext.IsRegularUser)
        {
            return query.Where(ticket => ticket.CreatedByUserId == userContext.UserId);
        }

        throw ApiException.Forbidden(
            "You are not allowed to access tickets with the current role set.",
            ErrorCodes.TicketUnauthorizedAccess);
    }

    public void EnsureCanCreateTicket(UserContext userContext)
    {
        if (userContext.IsAdmin || userContext.IsAgent || userContext.IsRegularUser)
        {
            return;
        }

        throw ApiException.Forbidden(
            "You are not allowed to create tickets.",
            ErrorCodes.TicketUnauthorizedAccess);
    }

    public void EnsureCanViewTicket(Ticket ticket, UserContext userContext)
    {
        if (userContext.IsAdmin)
        {
            return;
        }

        if (userContext.IsAgent
            && (string.Equals(ticket.AssignedToUserId, userContext.UserId, StringComparison.Ordinal)
                || string.Equals(ticket.CreatedByUserId, userContext.UserId, StringComparison.Ordinal)))
        {
            return;
        }

        if (string.Equals(ticket.CreatedByUserId, userContext.UserId, StringComparison.Ordinal))
        {
            return;
        }

        throw ApiException.Forbidden(
            "You are not allowed to access this ticket.",
            ErrorCodes.TicketUnauthorizedAccess);
    }

    public void EnsureCanManageTicket(Ticket ticket, UserContext userContext)
    {
        if (userContext.IsAdmin)
        {
            return;
        }

        if (userContext.IsAgent && string.Equals(ticket.AssignedToUserId, userContext.UserId, StringComparison.Ordinal))
        {
            return;
        }

        throw ApiException.Forbidden(
            "You are not allowed to manage this ticket.",
            ErrorCodes.TicketUnauthorizedAccess);
    }

    public void EnsureCanAssignTicket(UserContext userContext)
    {
        if (userContext.IsAdmin)
        {
            return;
        }

        throw ApiException.Forbidden(
            "You are not allowed to perform this action.",
            ErrorCodes.TicketUnauthorizedAccess);
    }
}
