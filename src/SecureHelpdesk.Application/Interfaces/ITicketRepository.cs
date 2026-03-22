using SecureHelpdesk.Domain.Entities;

namespace SecureHelpdesk.Application.Interfaces;

public interface ITicketRepository
{
    Task AddAsync(Ticket ticket, CancellationToken cancellationToken);
    Task AddCommentAsync(TicketComment comment, CancellationToken cancellationToken);
    Task AddAuditLogAsync(TicketAuditLog auditLog, CancellationToken cancellationToken);
    IQueryable<Ticket> QueryForList();
    Task<Ticket?> GetByIdWithDetailsAsync(Guid ticketId, bool asNoTracking, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
