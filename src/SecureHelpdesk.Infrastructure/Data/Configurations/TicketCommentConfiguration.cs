using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecureHelpdesk.Domain.Entities;

namespace SecureHelpdesk.Infrastructure.Data.Configurations;

public class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
{
    public void Configure(EntityTypeBuilder<TicketComment> entity)
    {
        entity.ToTable("TicketComments");

        entity.Property(c => c.Content)
            .HasMaxLength(1000)
            .IsRequired();

        entity.Property(c => c.CreatedAtUtc).IsRequired();
        entity.HasIndex(c => c.TicketId);

        entity.HasOne(c => c.Ticket)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(c => c.AuthorUser)
            .WithMany(u => u.AuthoredComments)
            .HasForeignKey(c => c.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
