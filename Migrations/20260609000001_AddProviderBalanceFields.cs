using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromptMarketPlace.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderBalanceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BalanceCurrency",
                table: "AiProviders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BalanceJsonPath",
                table: "AiProviders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BalanceUrl",
                table: "AiProviders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BalanceCurrency",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "BalanceJsonPath",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "BalanceUrl",
                table: "AiProviders");
        }
    }
}
