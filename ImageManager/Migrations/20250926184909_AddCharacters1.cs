using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ImageManager.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacters1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "characters",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_characters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CharacterImage",
                schema: "identity",
                columns: table => new
                {
                    CharactersId = table.Column<int>(type: "integer", nullable: false),
                    ImageId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterImage", x => new { x.CharactersId, x.ImageId });
                    table.ForeignKey(
                        name: "FK_CharacterImage_characters_CharactersId",
                        column: x => x.CharactersId,
                        principalSchema: "identity",
                        principalTable: "characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterImage_images_ImageId",
                        column: x => x.ImageId,
                        principalSchema: "identity",
                        principalTable: "images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CharacterImage_ImageId",
                schema: "identity",
                table: "CharacterImage",
                column: "ImageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacterImage",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "characters",
                schema: "identity");
        }
    }
}
