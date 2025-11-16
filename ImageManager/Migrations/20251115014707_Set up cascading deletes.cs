using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImageManager.Migrations
{
    /// <inheritdoc />
    public partial class Setupcascadingdeletes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadedImages_Images_ImageId",
                schema: "identity",
                table: "DownloadedImages");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadedImages_Images_ImageId",
                schema: "identity",
                table: "DownloadedImages",
                column: "ImageId",
                principalSchema: "identity",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadedImages_Images_ImageId",
                schema: "identity",
                table: "DownloadedImages");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadedImages_Images_ImageId",
                schema: "identity",
                table: "DownloadedImages",
                column: "ImageId",
                principalSchema: "identity",
                principalTable: "Images",
                principalColumn: "Id");
        }
    }
}
