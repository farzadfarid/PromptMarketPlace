using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Data.Configurations;

public class CreatorProfileConfiguration : IEntityTypeConfiguration<CreatorProfile>
{
    public void Configure(EntityTypeBuilder<CreatorProfile> builder)
    {
        builder.Property(c => c.CommissionPercent).HasPrecision(5, 2);
        builder.HasIndex(c => c.UserId).IsUnique();
    }
}
