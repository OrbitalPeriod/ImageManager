using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImageManager.Migrations
{
    /// <inheritdoc />
    public partial class Pain1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadedImages_Images_ImageId",
                schema: "identity",
                table: "DownloadedImages");

            migrationBuilder.DropIndex(
                name: "IX_DownloadedImages_ImageId",
                schema: "identity",
                table: "DownloadedImages");

            migrationBuilder.CreateIndex(
                name: "IX_Images_DownloadedImageId",
                schema: "identity",
                table: "Images",
                column: "DownloadedImageId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_DownloadedImages_DownloadedImageId",
                schema: "identity",
                table: "Images",
                column: "DownloadedImageId",
                principalSchema: "identity",
                principalTable: "DownloadedImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_DownloadedImages_DownloadedImageId",
                schema: "identity",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_DownloadedImageId",
                schema: "identity",
                table: "Images");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadedImages_ImageId",
                schema: "identity",
                table: "DownloadedImages",
                column: "ImageId",
                unique: true);

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
