using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecureHelpdesk.Domain.Entities;

namespace SecureHelpdesk.Infrastructure.Data.Configurations;

public class TicketAuditLogConfiguration : IEntityTypeConfiguration<TicketAuditLog>
{
    public void Configure(EntityTypeBuilder<TicketAuditLog> entity)
    {
        entity.ToTable("TicketAuditLogs");

        entity.Property(a => a.ActionType).IsRequired();
        entity.Property(a => a.OldValue).HasMaxLength(500);
        entity.Property(a => a.NewValue).HasMaxLength(500);
        entity.Property(a => a.ChangedByUserId).IsRequired();
        entity.Property(a => a.ChangedAtUtc).IsRequired();

        entity.HasIndex(a => a.TicketId);
        entity.HasIndex(a => a.ChangedByUserId);
        entity.HasIndex(a => new { a.TicketId, a.ChangedAtUtc });
        entity.HasIndex(a => new { a.TicketId, a.ActionType, a.ChangedAtUtc });

        entity.HasOne(a => a.Ticket)
            .WithMany(t => t.AuditLogs)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(a => a.ChangedByUser)
            .WithMany(u => u.ChangedAuditLogs)
            .HasForeignKey(a => a.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
