using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ImageManager.Migrations
{
    /// <inheritdoc />
    public partial class sharetokensagain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                schema: "identity",
                table: "images",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "User",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "text", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "shareTokens",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    ImageId = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Expires = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shareTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shareTokens_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_shareTokens_images_ImageId",
                        column: x => x.ImageId,
                        principalSchema: "identity",
                        principalTable: "images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_images_UserId",
                schema: "identity",
                table: "images",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_shareTokens_ImageId",
                schema: "identity",
                table: "shareTokens",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_shareTokens_UserId",
                schema: "identity",
                table: "shareTokens",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_images_User_UserId",
                schema: "identity",
                table: "images",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_images_User_UserId",
                schema: "identity",
                table: "images");

            migrationBuilder.DropTable(
                name: "shareTokens",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "User",
                schema: "identity");

            migrationBuilder.DropIndex(
                name: "IX_images_UserId",
                schema: "identity",
                table: "images");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "identity",
                table: "images");
        }
    }
}
