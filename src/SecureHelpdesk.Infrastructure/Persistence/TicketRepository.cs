using SecureHelpdesk.Application.Interfaces;
using SecureHelpdesk.Domain.Entities;

namespace SecureHelpdesk.Infrastructure.Persistence;

public class TicketRepository : ITicketRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TicketRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        return _dbContext.Tickets.AddAsync(ticket, cancellationToken).AsTask();
    }

    public IQueryable<Ticket> Query()
    {
        return _dbContext.Tickets.AsQueryable();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
