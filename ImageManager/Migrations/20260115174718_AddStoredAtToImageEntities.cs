using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImageManager.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredAtToImageEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StoredAt",
                schema: "identity",
                table: "UserOwnedImages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "StoredAt",
                schema: "identity",
                table: "Images",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "StoredAt",
                schema: "identity",
                table: "DownloadedImages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoredAt",
                schema: "identity",
                table: "UserOwnedImages");

            migrationBuilder.DropColumn(
                name: "StoredAt",
                schema: "identity",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "StoredAt",
                schema: "identity",
                table: "DownloadedImages");
        }
    }
}
