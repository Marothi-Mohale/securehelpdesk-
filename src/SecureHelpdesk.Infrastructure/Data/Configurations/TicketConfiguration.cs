using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecureHelpdesk.Domain.Entities;

namespace SecureHelpdesk.Infrastructure.Data.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> entity)
    {
        entity.ToTable("Tickets");

        entity.Property(t => t.Title)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(t => t.Description)
            .HasMaxLength(4000)
            .IsRequired();

        entity.Property(t => t.CreatedAtUtc).IsRequired();
        entity.HasIndex(t => t.Status);
        entity.HasIndex(t => t.Priority);
        entity.HasIndex(t => t.AssignedToUserId);
        entity.HasIndex(t => t.CreatedByUserId);
        entity.HasIndex(t => new { t.Status, t.Priority, t.AssignedToUserId });

        entity.HasOne(t => t.CreatedByUser)
            .WithMany(u => u.CreatedTickets)
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(t => t.AssignedToUser)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
