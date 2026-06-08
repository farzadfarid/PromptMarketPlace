using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);

        builder.HasOne(u => u.Wallet)
            .WithOne(w => w.User)
            .HasForeignKey<UserWallet>(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.CreatorProfile)
            .WithOne(c => c.User)
            .HasForeignKey<CreatorProfile>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Executions)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Transactions)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
