using SecureHelpdesk.Domain.Entities;

namespace SecureHelpdesk.Application.Interfaces;

public interface ITicketRepository
{
    Task AddAsync(Ticket ticket, CancellationToken cancellationToken);
    IQueryable<Ticket> Query();
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
