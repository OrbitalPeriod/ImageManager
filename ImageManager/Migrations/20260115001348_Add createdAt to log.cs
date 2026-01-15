using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImageManager.Migrations
{
    /// <inheritdoc />
    public partial class AddcreatedAttolog : Migration
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

            migrationBuilder.DropColumn(
                name: "ImageId",
                schema: "identity",
                table: "DownloadedImages");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "identity",
                table: "PlatformSyncLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "DownloadedImageId",
                schema: "identity",
                table: "Images",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_DownloadedImageId",
                schema: "identity",
                table: "Images",
                column: "DownloadedImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_DownloadedImages_DownloadedImageId",
                schema: "identity",
                table: "Images",
                column: "DownloadedImageId",
                principalSchema: "identity",
                principalTable: "DownloadedImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
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

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "identity",
                table: "PlatformSyncLogs");

            migrationBuilder.DropColumn(
                name: "DownloadedImageId",
                schema: "identity",
                table: "Images");

            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                schema: "identity",
                table: "DownloadedImages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
