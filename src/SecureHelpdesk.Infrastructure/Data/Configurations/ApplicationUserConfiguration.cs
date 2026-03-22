using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecureHelpdesk.Domain.Entities;

namespace SecureHelpdesk.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> entity)
    {
        entity.Property(u => u.FullName)
            .HasMaxLength(120)
            .IsRequired();

        entity.HasIndex(u => u.FullName);
    }
}
