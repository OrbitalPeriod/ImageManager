using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImageManager.Migrations
{
    /// <inheritdoc />
    public partial class sharetokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TagName",
                schema: "identity",
                table: "tags",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "identity",
                table: "characters",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TagName",
                schema: "identity",
                table: "tags");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "identity",
                table: "characters");
        }
    }
}
