using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImageManager.Migrations
{
    /// <inheritdoc />
    public partial class DidALot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadedImages_Images_ImageId",
                schema: "identity",
                table: "DownloadedImages");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_AspNetUsers_UserId",
                schema: "identity",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_ShareTokens_Images_ImageId",
                schema: "identity",
                table: "ShareTokens");

            migrationBuilder.DropIndex(
                name: "IX_Images_UserId",
                schema: "identity",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_DownloadedImages_ImageId",
                schema: "identity",
                table: "DownloadedImages");

            migrationBuilder.DropColumn(
                name: "DownloadedImageId",
                schema: "identity",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "Publicity",
                schema: "identity",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "identity",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "ImageId",
                schema: "identity",
                table: "DownloadedImages");

            migrationBuilder.RenameColumn(
                name: "ImageId",
                schema: "identity",
                table: "ShareTokens",
                newName: "UserOwnedImageId");

            migrationBuilder.RenameIndex(
                name: "IX_ShareTokens_ImageId",
                schema: "identity",
                table: "ShareTokens",
                newName: "IX_ShareTokens_UserOwnedImageId");

            migrationBuilder.AddColumn<Guid>(
                name: "UserOwnedImageId",
                schema: "identity",
                table: "DownloadedImages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "UserOwnedImages",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Publicity = table.Column<int>(type: "integer", nullable: false),
                    DownloadedImageId = table.Column<int>(type: "integer", nullable: false),
                    ImageId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOwnedImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserOwnedImages_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserOwnedImages_Images_ImageId",
                        column: x => x.ImageId,
                        principalSchema: "identity",
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadedImages_UserOwnedImageId",
                schema: "identity",
                table: "DownloadedImages",
                column: "UserOwnedImageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserOwnedImages_ImageId",
                schema: "identity",
                table: "UserOwnedImages",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOwnedImages_UserId",
                schema: "identity",
                table: "UserOwnedImages",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadedImages_UserOwnedImages_UserOwnedImageId",
                schema: "identity",
                table: "DownloadedImages",
                column: "UserOwnedImageId",
                principalSchema: "identity",
                principalTable: "UserOwnedImages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShareTokens_UserOwnedImages_UserOwnedImageId",
                schema: "identity",
                table: "ShareTokens",
                column: "UserOwnedImageId",
                principalSchema: "identity",
                principalTable: "UserOwnedImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadedImages_UserOwnedImages_UserOwnedImageId",
                schema: "identity",
                table: "DownloadedImages");

            migrationBuilder.DropForeignKey(
                name: "FK_ShareTokens_UserOwnedImages_UserOwnedImageId",
                schema: "identity",
                table: "ShareTokens");

            migrationBuilder.DropTable(
                name: "UserOwnedImages",
                schema: "identity");

            migrationBuilder.DropIndex(
                name: "IX_DownloadedImages_UserOwnedImageId",
                schema: "identity",
                table: "DownloadedImages");

            migrationBuilder.DropColumn(
                name: "UserOwnedImageId",
                schema: "identity",
                table: "DownloadedImages");

            migrationBuilder.RenameColumn(
                name: "UserOwnedImageId",
                schema: "identity",
                table: "ShareTokens",
                newName: "ImageId");

            migrationBuilder.RenameIndex(
                name: "IX_ShareTokens_UserOwnedImageId",
                schema: "identity",
                table: "ShareTokens",
                newName: "IX_ShareTokens_ImageId");

            migrationBuilder.AddColumn<int>(
                name: "DownloadedImageId",
                schema: "identity",
                table: "Images",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Publicity",
                schema: "identity",
                table: "Images",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                schema: "identity",
                table: "Images",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                schema: "identity",
                table: "DownloadedImages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_UserId",
                schema: "identity",
                table: "Images",
                column: "UserId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Images_AspNetUsers_UserId",
                schema: "identity",
                table: "Images",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShareTokens_Images_ImageId",
                schema: "identity",
                table: "ShareTokens",
                column: "ImageId",
                principalSchema: "identity",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
