using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImageManager.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintUserOwnedImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove duplicate UserOwnedImage entries, keeping only the oldest one for each (ImageId, UserId) pair
            migrationBuilder.Sql(@"
                DELETE FROM identity.""UserOwnedImages""
                WHERE ""Id"" NOT IN (
                    SELECT DISTINCT ON (""ImageId"", ""UserId"") ""Id""
                    FROM identity.""UserOwnedImages""
                    ORDER BY ""ImageId"", ""UserId"", ""StoredAt"" ASC
                );
            ");

            migrationBuilder.DropIndex(
                name: "IX_UserOwnedImages_ImageId",
                schema: "identity",
                table: "UserOwnedImages");

            migrationBuilder.CreateIndex(
                name: "IX_UserOwnedImages_ImageId_UserId",
                schema: "identity",
                table: "UserOwnedImages",
                columns: new[] { "ImageId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserOwnedImages_ImageId_UserId",
                schema: "identity",
                table: "UserOwnedImages");

            migrationBuilder.CreateIndex(
                name: "IX_UserOwnedImages_ImageId",
                schema: "identity",
                table: "UserOwnedImages",
                column: "ImageId");
        }
    }
}
