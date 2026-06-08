using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Data.Configurations;

public class AiAppConfiguration : IEntityTypeConfiguration<AiApp>
{
    public void Configure(EntityTypeBuilder<AiApp> builder)
    {
        builder.HasIndex(a => a.Slug).IsUnique();
        builder.Property(a => a.Slug).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Title).HasMaxLength(200).IsRequired();
        builder.Property(a => a.ShortDescription).HasMaxLength(160).IsRequired();
        builder.Property(a => a.AverageRating).HasPrecision(3, 2);

        builder.HasOne(a => a.AiModel)
            .WithMany(m => m.Apps)
            .HasForeignKey(a => a.AiModelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Category)
            .WithMany(c => c.Apps)
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Creator)
            .WithMany(c => c.Apps)
            .HasForeignKey(a => a.CreatorProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
