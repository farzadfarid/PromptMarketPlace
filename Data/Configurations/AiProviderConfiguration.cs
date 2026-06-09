using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Data.Configurations;

public class AiProviderConfiguration : IEntityTypeConfiguration<AiProvider>
{
    public void Configure(EntityTypeBuilder<AiProvider> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.BaseUrl).HasMaxLength(500).IsRequired();
        builder.Property(p => p.BalanceUrl).HasMaxLength(500);
        builder.Property(p => p.BalanceJsonPath).HasMaxLength(200);
        builder.Property(p => p.BalanceCurrency).HasMaxLength(20);
        builder.Property(p => p.IsActiveForText).HasDefaultValue(false);
        builder.Property(p => p.IsActiveForImage).HasDefaultValue(false);
        builder.Property(p => p.IsActiveForVideo).HasDefaultValue(false);
        builder.Property(p => p.IsActiveForAudio).HasDefaultValue(false);

        builder.HasData(new AiProvider
        {
            Id = 1,
            Name = "OpenRouter",
            BaseUrl = "https://openrouter.ai/api/v1",
            Description = "گیت‌وی یکپارچه برای دسترسی به همه مدل‌های هوش مصنوعی",
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
