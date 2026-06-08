using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Data.Configurations;

public class AdminAuditLogConfiguration : IEntityTypeConfiguration<AdminAuditLog>
{
    public void Configure(EntityTypeBuilder<AdminAuditLog> builder)
    {
        builder.Property(a => a.Action).HasMaxLength(200).IsRequired();
        builder.Property(a => a.TargetType).HasMaxLength(50);
        builder.Property(a => a.TargetId).HasMaxLength(100);
        builder.Property(a => a.IpAddress).HasMaxLength(50);

        builder.HasOne(a => a.Admin)
            .WithMany()
            .HasForeignKey(a => a.AdminUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => a.AdminUserId);
    }
}
