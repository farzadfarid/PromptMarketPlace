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
