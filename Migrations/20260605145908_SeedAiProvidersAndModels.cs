using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PromptMarketPlace.Migrations
{
    /// <inheritdoc />
    public partial class SeedAiProvidersAndModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AiProviders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "BaseUrl",
                table: "AiProviders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AiModels",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ModelId",
                table: "AiModels",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CostPerSecondVideo",
                table: "AiModels",
                type: "decimal(10,6)",
                precision: 10,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CostPerImage",
                table: "AiModels",
                type: "decimal(10,6)",
                precision: 10,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CostPer1KTokens",
                table: "AiModels",
                type: "decimal(10,6)",
                precision: 10,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "AiProviders",
                columns: new[] { "Id", "ApiKeyEncrypted", "BaseUrl", "CreatedAt", "Description", "IsActive", "Name" },
                values: new object[] { 1, null, "https://openrouter.ai/api/v1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "گیت‌وی یکپارچه برای دسترسی به همه مدل‌های هوش مصنوعی", true, "OpenRouter" });

            migrationBuilder.InsertData(
                table: "AiModels",
                columns: new[] { "Id", "AiProviderId", "Capabilities", "CostPer1KTokens", "CostPerImage", "CostPerSecondVideo", "Description", "IsActive", "IsDefault", "MaxTokens", "ModelId", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, 1, "[\"TextGeneration\",\"CodeGeneration\"]", 0.003m, null, null, null, true, true, 200000, "anthropic/claude-sonnet-4-6", "Claude Sonnet 4.6", 1 },
                    { 2, 1, "[\"TextGeneration\",\"CodeGeneration\"]", 0.00025m, null, null, null, true, false, 200000, "anthropic/claude-haiku-4-5", "Claude Haiku 4.5", 2 },
                    { 3, 1, "[\"TextGeneration\",\"CodeGeneration\"]", 0.005m, null, null, null, true, false, 128000, "openai/gpt-4o", "GPT-4o", 3 },
                    { 4, 1, "[\"TextGeneration\",\"CodeGeneration\"]", 0.00015m, null, null, null, true, false, 128000, "openai/gpt-4o-mini", "GPT-4o Mini", 4 },
                    { 5, 1, "[\"TextGeneration\",\"CodeGeneration\"]", 0.00125m, null, null, null, true, false, 1000000, "google/gemini-2.5-pro", "Gemini 2.5 Pro", 5 },
                    { 6, 1, "[\"ImageGeneration\"]", null, 0.04m, null, null, true, true, null, "black-forest-labs/flux-1.1-pro", "FLUX 1.1 Pro", 10 },
                    { 7, 1, "[\"ImageGeneration\"]", null, 0.003m, null, null, true, false, null, "black-forest-labs/flux-schnell", "FLUX Schnell", 11 },
                    { 8, 1, "[\"ImageGeneration\"]", null, 0.035m, null, null, true, false, null, "stabilityai/stable-diffusion-3-5", "Stable Diffusion 3.5", 12 },
                    { 9, 1, "[\"ImageGeneration\"]", null, 0.04m, null, null, true, false, null, "openai/dall-e-3", "DALL-E 3", 13 },
                    { 10, 1, "[\"VideoGeneration\"]", null, null, 0.14m, null, true, true, null, "kling/kling-1-6-pro", "Kling 1.6 Pro", 20 },
                    { 11, 1, "[\"VideoGeneration\"]", null, null, 0.05m, null, true, false, null, "runway/gen-4", "Runway Gen-4", 21 },
                    { 12, 1, "[\"VideoGeneration\"]", null, null, 0.003m, null, true, false, null, "luma/dream-machine", "Luma Dream Machine", 22 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AiModels",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AiModels",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AiModels",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AiModels",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AiModels",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AiModels",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AiModels",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AiModels",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AiModels",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AiModels",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "AiModels",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "AiModels",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "AiProviders",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AiProviders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "BaseUrl",
                table: "AiProviders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AiModels",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "ModelId",
                table: "AiModels",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<decimal>(
                name: "CostPerSecondVideo",
                table: "AiModels",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,6)",
                oldPrecision: 10,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CostPerImage",
                table: "AiModels",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,6)",
                oldPrecision: 10,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CostPer1KTokens",
                table: "AiModels",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,6)",
                oldPrecision: 10,
                oldScale: 6,
                oldNullable: true);
        }
    }
}
