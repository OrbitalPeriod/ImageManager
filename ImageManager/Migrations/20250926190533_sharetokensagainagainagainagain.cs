using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImageManager.Migrations
{
    /// <inheritdoc />
    public partial class sharetokensagainagainagainagain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_images_User_UserId",
                schema: "identity",
                table: "images");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "identity",
                table: "images",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_images_User_UserId",
                schema: "identity",
                table: "images",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_images_User_UserId",
                schema: "identity",
                table: "images");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "identity",
                table: "images",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_images_User_UserId",
                schema: "identity",
                table: "images",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "User",
                principalColumn: "Id");
        }
    }
}
