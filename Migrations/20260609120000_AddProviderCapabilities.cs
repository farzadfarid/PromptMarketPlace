using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromptMarketPlace.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderCapabilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActiveForAudio",
                table: "AiProviders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActiveForImage",
                table: "AiProviders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActiveForText",
                table: "AiProviders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActiveForVideo",
                table: "AiProviders",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsActiveForAudio", table: "AiProviders");
            migrationBuilder.DropColumn(name: "IsActiveForImage", table: "AiProviders");
            migrationBuilder.DropColumn(name: "IsActiveForText", table: "AiProviders");
            migrationBuilder.DropColumn(name: "IsActiveForVideo", table: "AiProviders");
        }
    }
}
