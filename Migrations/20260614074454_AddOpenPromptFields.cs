using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromptMarketPlace.Migrations
{
    /// <inheritdoc />
    public partial class AddOpenPromptFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPromptPublic",
                table: "Apps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPromptPublicRequested",
                table: "Apps",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPromptPublic",
                table: "Apps");

            migrationBuilder.DropColumn(
                name: "IsPromptPublicRequested",
                table: "Apps");
        }
    }
}
