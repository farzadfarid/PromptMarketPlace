using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Data.Configurations;

public class CreditPackageConfiguration : IEntityTypeConfiguration<CreditPackage>
{
    public void Configure(EntityTypeBuilder<CreditPackage> builder)
    {
        builder.Property(p => p.PriceRial).HasPrecision(18, 2);

        builder.HasData(
            new CreditPackage { Id = 1, Name = "بسته آزمایشی", CreditAmount = 50, PriceRial = 250000, IsActive = true, SortOrder = 1 },
            new CreditPackage { Id = 2, Name = "بسته استارتر", CreditAmount = 150, PriceRial = 600000, IsActive = true, SortOrder = 2 },
            new CreditPackage { Id = 3, Name = "بسته حرفه‌ای", CreditAmount = 400, PriceRial = 1400000, IsActive = true, IsBestValue = true, SortOrder = 3 },
            new CreditPackage { Id = 4, Name = "بسته سازمانی", CreditAmount = 1000, PriceRial = 3000000, IsActive = true, SortOrder = 4 }
        );
    }
}
