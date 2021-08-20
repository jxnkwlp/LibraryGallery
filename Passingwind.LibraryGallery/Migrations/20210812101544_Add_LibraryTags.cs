using Microsoft.EntityFrameworkCore.Migrations;

namespace Passingwind.LibraryGallery.Migrations
{
    public partial class Add_LibraryTags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LibraryTags",
                columns: table => new
                {
                    LibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryTags", x => new { x.LibraryId, x.Name });
                    table.ForeignKey(
                        name: "FK_LibraryTags_Libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Libraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LibraryTags");
        }
    }
}
