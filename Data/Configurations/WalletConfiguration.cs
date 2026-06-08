using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Data.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<UserWallet>
{
    public void Configure(EntityTypeBuilder<UserWallet> builder)
    {
        builder.Property(w => w.EarningBalance).HasPrecision(18, 2);
        builder.Property(w => w.TotalEarned).HasPrecision(18, 2);
        builder.Property(w => w.TotalWithdrawn).HasPrecision(18, 2);
        builder.HasIndex(w => w.UserId).IsUnique();
    }
}
