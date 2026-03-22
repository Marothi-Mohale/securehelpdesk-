using SecureHelpdesk.Application.Common;
using SecureHelpdesk.Domain.Entities;

namespace SecureHelpdesk.Application.Interfaces;

public interface ITicketAuthorizationService
{
    IQueryable<Ticket> ApplyListScope(IQueryable<Ticket> query, UserContext userContext);
    void EnsureCanCreateTicket(UserContext userContext);
    void EnsureCanViewTicket(Ticket ticket, UserContext userContext);
    void EnsureCanManageTicket(Ticket ticket, UserContext userContext);
    void EnsureCanAssignTicket(UserContext userContext);
}
