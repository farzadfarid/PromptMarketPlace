using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromptMarketPlace.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProviderType",
                table: "AiProviders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AiProviders",
                keyColumn: "Id",
                keyValue: 1,
                column: "ProviderType",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProviderType",
                table: "AiProviders");
        }
    }
}
