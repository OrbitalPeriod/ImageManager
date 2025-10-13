using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImageManager.Migrations
{
    /// <inheritdoc />
    public partial class Dunno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Token",
                schema: "identity",
                table: "ShareTokens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Token",
                schema: "identity",
                table: "ShareTokens",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
