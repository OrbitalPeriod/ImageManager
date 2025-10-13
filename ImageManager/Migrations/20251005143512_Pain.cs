using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImageManager.Migrations
{
    /// <inheritdoc />
    public partial class Pain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadedImages_AspNetUsers_UserId1",
                schema: "identity",
                table: "DownloadedImages");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadedImages_UserOwnedImages_UserOwnedImageId",
                schema: "identity",
                table: "DownloadedImages");

            migrationBuilder.DropIndex(
                name: "IX_DownloadedImages_UserId1",
                schema: "identity",
                table: "DownloadedImages");

            migrationBuilder.DropColumn(
                name: "UserId1",
                schema: "identity",
                table: "DownloadedImages");

            migrationBuilder.RenameColumn(
                name: "UserOwnedImageId",
                schema: "identity",
                table: "DownloadedImages",
                newName: "ImageId");

            migrationBuilder.RenameIndex(
                name: "IX_DownloadedImages_UserOwnedImageId",
                schema: "identity",
                table: "DownloadedImages",
                newName: "IX_DownloadedImages_ImageId");

            migrationBuilder.AddColumn<int>(
                name: "DownloadedImageId",
                schema: "identity",
                table: "Images",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "identity",
                table: "DownloadedImages",
                type: "text",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_UserOwnedImages_DownloadedImageId",
                schema: "identity",
                table: "UserOwnedImages",
                column: "DownloadedImageId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadedImages_UserId",
                schema: "identity",
                table: "DownloadedImages",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadedImages_AspNetUsers_UserId",
                schema: "identity",
                table: "DownloadedImages",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadedImages_Images_ImageId",
                schema: "identity",
                table: "DownloadedImages",
                column: "ImageId",
                principalSchema: "identity",
                principalTable: "Images",
                principalColumn: "Id");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadedImages_AspNetUsers_UserId",
                schema: "identity",
                table: "DownloadedImages");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadedImages_Images_ImageId",
                schema: "identity",
                table: "DownloadedImages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserOwnedImages_DownloadedImages_DownloadedImageId",
                schema: "identity",
                table: "UserOwnedImages");

            migrationBuilder.DropIndex(
                name: "IX_UserOwnedImages_DownloadedImageId",
                schema: "identity",
                table: "UserOwnedImages");

            migrationBuilder.DropIndex(
                name: "IX_DownloadedImages_UserId",
                schema: "identity",
                table: "DownloadedImages");

            migrationBuilder.DropColumn(
                name: "DownloadedImageId",
                schema: "identity",
                table: "Images");

            migrationBuilder.RenameColumn(
                name: "ImageId",
                schema: "identity",
                table: "DownloadedImages",
                newName: "UserOwnedImageId");

            migrationBuilder.RenameIndex(
                name: "IX_DownloadedImages_ImageId",
                schema: "identity",
                table: "DownloadedImages",
                newName: "IX_DownloadedImages_UserOwnedImageId");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                schema: "identity",
                table: "DownloadedImages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                schema: "identity",
                table: "DownloadedImages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadedImages_UserId1",
                schema: "identity",
                table: "DownloadedImages",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadedImages_AspNetUsers_UserId1",
                schema: "identity",
                table: "DownloadedImages",
                column: "UserId1",
                principalSchema: "identity",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadedImages_UserOwnedImages_UserOwnedImageId",
                schema: "identity",
                table: "DownloadedImages",
                column: "UserOwnedImageId",
                principalSchema: "identity",
                principalTable: "UserOwnedImages",
                principalColumn: "Id");
        }
    }
}
