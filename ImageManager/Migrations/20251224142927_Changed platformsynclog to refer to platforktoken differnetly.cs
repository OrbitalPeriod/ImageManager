using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImageManager.Migrations
{
    /// <inheritdoc />
    public partial class Changedplatformsynclogtorefertoplatforktokendiffernetly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlatformSyncLogs",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlatformTokenId = table.Column<Guid>(type: "uuid", nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformSyncLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformSyncLogs_PlatformTokens_PlatformTokenId",
                        column: x => x.PlatformTokenId,
                        principalSchema: "identity",
                        principalTable: "PlatformTokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformSyncLogs_PlatformTokenId",
                schema: "identity",
                table: "PlatformSyncLogs",
                column: "PlatformTokenId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformSyncLogs",
                schema: "identity");
        }
    }
}
