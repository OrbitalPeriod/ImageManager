using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImageManager.Migrations
{
    /// <inheritdoc />
    public partial class Pain123 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserOwnedImages_DownloadedImages_DownloadedImageId",
                schema: "identity",
                table: "UserOwnedImages");

            migrationBuilder.DropIndex(
                name: "IX_UserOwnedImages_DownloadedImageId",
                schema: "identity",
                table: "UserOwnedImages");

            migrationBuilder.DropColumn(
                name: "DownloadedImageId",
                schema: "identity",
                table: "UserOwnedImages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DownloadedImageId",
                schema: "identity",
                table: "UserOwnedImages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserOwnedImages_DownloadedImageId",
                schema: "identity",
                table: "UserOwnedImages",
                column: "DownloadedImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserOwnedImages_DownloadedImages_DownloadedImageId",
                schema: "identity",
                table: "UserOwnedImages",
                column: "DownloadedImageId",
                principalSchema: "identity",
                principalTable: "DownloadedImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
