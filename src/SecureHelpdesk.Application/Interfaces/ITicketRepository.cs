using SecureHelpdesk.Domain.Entities;

namespace SecureHelpdesk.Application.Interfaces;

public interface ITicketRepository
{
    Task AddAsync(Ticket ticket, CancellationToken cancellationToken);
    IQueryable<Ticket> QueryForList();
    Task<Ticket?> GetByIdWithDetailsAsync(Guid ticketId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
