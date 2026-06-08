using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Data.Configurations;

public class AppCategoryConfiguration : IEntityTypeConfiguration<AppCategory>
{
    public void Configure(EntityTypeBuilder<AppCategory> builder)
    {
        builder.HasIndex(c => c.Slug).IsUnique();
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(100).IsRequired();

        builder.HasData(
            new AppCategory { Id = 1, Name = "تولید محتوا", Slug = "content-creation", IconClass = "fas fa-pen-nib", SortOrder = 1 },
            new AppCategory { Id = 2, Name = "مارکتینگ و تبلیغات", Slug = "marketing", IconClass = "fas fa-bullhorn", SortOrder = 2 },
            new AppCategory { Id = 3, Name = "تصویر و گرافیک", Slug = "image-graphics", IconClass = "fas fa-image", SortOrder = 3 },
            new AppCategory { Id = 4, Name = "ویدیو", Slug = "video", IconClass = "fas fa-video", SortOrder = 4 },
            new AppCategory { Id = 5, Name = "برنامه‌نویسی", Slug = "programming", IconClass = "fas fa-code", SortOrder = 5 },
            new AppCategory { Id = 6, Name = "حقوقی و قراردادها", Slug = "legal", IconClass = "fas fa-gavel", SortOrder = 6 },
            new AppCategory { Id = 7, Name = "کسب‌وکار و مالی", Slug = "business", IconClass = "fas fa-briefcase", SortOrder = 7 },
            new AppCategory { Id = 8, Name = "آموزش", Slug = "education", IconClass = "fas fa-graduation-cap", SortOrder = 8 }
        );
    }
}
