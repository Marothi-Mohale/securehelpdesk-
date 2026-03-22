using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecureHelpdesk.Domain.Entities;

namespace SecureHelpdesk.Infrastructure.Data.Configurations;

public class TicketAuditHistoryConfiguration : IEntityTypeConfiguration<TicketAuditHistory>
{
    public void Configure(EntityTypeBuilder<TicketAuditHistory> entity)
    {
        entity.ToTable("TicketAuditHistory");

        entity.Property(a => a.Description)
            .HasMaxLength(500)
            .IsRequired();

        entity.HasIndex(a => a.TicketId);

        entity.HasOne(a => a.Ticket)
            .WithMany(t => t.AuditHistory)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(a => a.PerformedByUser)
            .WithMany(u => u.AuditEntries)
            .HasForeignKey(a => a.PerformedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
