using SecureHelpdesk.Application.Interfaces;
using SecureHelpdesk.Domain.Entities;
using SecureHelpdesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SecureHelpdesk.Infrastructure.Repositories;

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

    public IQueryable<Ticket> QueryForList()
    {
        return _dbContext.Tickets
            .AsNoTracking()
            .AsQueryable();
    }

    public Task<Ticket?> GetByIdWithDetailsAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        return _dbContext.Tickets
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Comments)
                .ThenInclude(c => c.AuthorUser)
            .Include(t => t.AuditLogs)
                .ThenInclude(a => a.ChangedByUser)
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
