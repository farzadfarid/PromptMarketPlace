using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Data.Configurations;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.HasIndex(s => s.Key).IsUnique();
        builder.Property(s => s.Key).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Group).HasMaxLength(50).IsRequired();

        builder.HasData(
            new SystemSetting { Id = 1, Key = "ZarinPal:MerchantId", Value = "", Group = "ZarinPal", Description = "شناسه پذیرنده زرین‌پال", IsEncrypted = true },
            new SystemSetting { Id = 2, Key = "ZarinPal:IsSandbox", Value = "true", Group = "ZarinPal", Description = "حالت تست (true) یا production (false)" },
            new SystemSetting { Id = 3, Key = "General:SiteName", Value = "پرامپت مارکت", Group = "General", Description = "نام سایت" },
            new SystemSetting { Id = 4, Key = "General:SupportEmail", Value = "", Group = "General", Description = "ایمیل پشتیبانی" },
            new SystemSetting { Id = 5, Key = "Commission:PlatformPercent", Value = "30", Group = "Commission", Description = "درصد کمیسیون پلتفرم" },
            new SystemSetting { Id = 6, Key = "Commission:FoundingCreatorPercent", Value = "15", Group = "Commission", Description = "درصد کمیسیون سازندگان بنیان‌گذار" },
            new SystemSetting { Id = 7, Key = "Withdrawal:MinimumAmount", Value = "500000", Group = "General", Description = "حداقل مبلغ برداشت (ریال)" },
            new SystemSetting { Id = 8, Key = "Registration:AutoApproveCreator", Value = "true", Group = "General", Description = "تایید خودکار سازندگان جدید" }
        );
    }
}
