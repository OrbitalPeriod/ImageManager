using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImageManager.Migrations
{
    /// <inheritdoc />
    public partial class AddFolderSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Folders",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Folders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FolderImages",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    FolderId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserOwnedImageId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FolderImages_Folders_FolderId",
                        column: x => x.FolderId,
                        principalSchema: "identity",
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FolderImages_UserOwnedImages_UserOwnedImageId",
                        column: x => x.UserOwnedImageId,
                        principalSchema: "identity",
                        principalTable: "UserOwnedImages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FolderImages_FolderId_UserOwnedImageId",
                schema: "identity",
                table: "FolderImages",
                columns: new[] { "FolderId", "UserOwnedImageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FolderImages_UserOwnedImageId",
                schema: "identity",
                table: "FolderImages",
                column: "UserOwnedImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_UserId",
                schema: "identity",
                table: "Folders",
                column: "UserId");

            // Create "Liked" folder for all existing users
            migrationBuilder.Sql(@"
                INSERT INTO identity.""Folders"" (""Id"", ""Name"", ""UserId"")
                SELECT gen_random_uuid(), 'Liked', ""Id""
                FROM identity.""AspNetUsers""
                WHERE ""Id"" NOT IN (
                    SELECT ""UserId"" FROM identity.""Folders"" WHERE ""Name"" = 'Liked'
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FolderImages",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "Folders",
                schema: "identity");
        }
    }
}
