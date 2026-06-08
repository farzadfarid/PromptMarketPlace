using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Data.Configurations;

public class AiModelConfiguration : IEntityTypeConfiguration<AiModel>
{
    public void Configure(EntityTypeBuilder<AiModel> builder)
    {
        builder.Property(m => m.Name).HasMaxLength(150).IsRequired();
        builder.Property(m => m.ModelId).HasMaxLength(200).IsRequired();
        builder.Property(m => m.CostPer1KTokens).HasPrecision(10, 6);
        builder.Property(m => m.CostPerImage).HasPrecision(10, 6);
        builder.Property(m => m.CostPerSecondVideo).HasPrecision(10, 6);

        builder.HasData(
            // ─── متنی و کد ───────────────────────────────────────
            new AiModel
            {
                Id = 1, AiProviderId = 1,
                Name = "Claude Sonnet 4.6",
                ModelId = "anthropic/claude-sonnet-4-6",
                Capabilities = "[\"TextGeneration\",\"CodeGeneration\"]",
                CostPer1KTokens = 0.003m,
                MaxTokens = 200000,
                IsActive = true, IsDefault = true, SortOrder = 1
            },
            new AiModel
            {
                Id = 2, AiProviderId = 1,
                Name = "Claude Haiku 4.5",
                ModelId = "anthropic/claude-haiku-4-5",
                Capabilities = "[\"TextGeneration\",\"CodeGeneration\"]",
                CostPer1KTokens = 0.00025m,
                MaxTokens = 200000,
                IsActive = true, SortOrder = 2
            },
            new AiModel
            {
                Id = 3, AiProviderId = 1,
                Name = "GPT-4o",
                ModelId = "openai/gpt-4o",
                Capabilities = "[\"TextGeneration\",\"CodeGeneration\"]",
                CostPer1KTokens = 0.005m,
                MaxTokens = 128000,
                IsActive = true, SortOrder = 3
            },
            new AiModel
            {
                Id = 4, AiProviderId = 1,
                Name = "GPT-4o Mini",
                ModelId = "openai/gpt-4o-mini",
                Capabilities = "[\"TextGeneration\",\"CodeGeneration\"]",
                CostPer1KTokens = 0.00015m,
                MaxTokens = 128000,
                IsActive = true, SortOrder = 4
            },
            new AiModel
            {
                Id = 5, AiProviderId = 1,
                Name = "Gemini 2.5 Pro",
                ModelId = "google/gemini-2.5-pro",
                Capabilities = "[\"TextGeneration\",\"CodeGeneration\"]",
                CostPer1KTokens = 0.00125m,
                MaxTokens = 1000000,
                IsActive = true, SortOrder = 5
            },

            // ─── تصویری ──────────────────────────────────────────
            new AiModel
            {
                Id = 6, AiProviderId = 1,
                Name = "FLUX 1.1 Pro",
                ModelId = "black-forest-labs/flux-1.1-pro",
                Capabilities = "[\"ImageGeneration\"]",
                CostPerImage = 0.04m,
                IsActive = true, IsDefault = true, SortOrder = 10
            },
            new AiModel
            {
                Id = 7, AiProviderId = 1,
                Name = "FLUX Schnell",
                ModelId = "black-forest-labs/flux-schnell",
                Capabilities = "[\"ImageGeneration\"]",
                CostPerImage = 0.003m,
                IsActive = true, SortOrder = 11
            },
            new AiModel
            {
                Id = 8, AiProviderId = 1,
                Name = "Stable Diffusion 3.5",
                ModelId = "stabilityai/stable-diffusion-3-5",
                Capabilities = "[\"ImageGeneration\"]",
                CostPerImage = 0.035m,
                IsActive = true, SortOrder = 12
            },
            new AiModel
            {
                Id = 9, AiProviderId = 1,
                Name = "DALL-E 3",
                ModelId = "openai/dall-e-3",
                Capabilities = "[\"ImageGeneration\"]",
                CostPerImage = 0.04m,
                IsActive = true, SortOrder = 13
            },

            // ─── ویدیویی ──────────────────────────────────────────
            new AiModel
            {
                Id = 10, AiProviderId = 1,
                Name = "Kling 1.6 Pro",
                ModelId = "kling/kling-1-6-pro",
                Capabilities = "[\"VideoGeneration\"]",
                CostPerSecondVideo = 0.14m,
                IsActive = true, IsDefault = true, SortOrder = 20
            },
            new AiModel
            {
                Id = 11, AiProviderId = 1,
                Name = "Runway Gen-4",
                ModelId = "runway/gen-4",
                Capabilities = "[\"VideoGeneration\"]",
                CostPerSecondVideo = 0.05m,
                IsActive = true, SortOrder = 21
            },
            new AiModel
            {
                Id = 12, AiProviderId = 1,
                Name = "Luma Dream Machine",
                ModelId = "luma/dream-machine",
                Capabilities = "[\"VideoGeneration\"]",
                CostPerSecondVideo = 0.003m,
                IsActive = true, SortOrder = 22
            }
        );
    }
}
