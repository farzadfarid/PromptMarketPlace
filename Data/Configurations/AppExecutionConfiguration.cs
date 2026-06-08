using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Data.Configurations;

public class AppExecutionConfiguration : IEntityTypeConfiguration<AppExecution>
{
    public void Configure(EntityTypeBuilder<AppExecution> builder)
    {
        builder.Property(e => e.ActualApiCost).HasPrecision(10, 6);

        builder.HasOne(e => e.App)
            .WithMany(a => a.Executions)
            .HasForeignKey(e => e.AppId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.UserId, e.CreatedAt });
        builder.HasIndex(e => new { e.AppId, e.CreatedAt });
    }
}
