using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthorLM_API.Migrations
{
    /// <inheritdoc />
    public partial class TryAddLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFavoriteBooks",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavoriteBooks", x => new { x.UserId, x.BookId });
                });

            migrationBuilder.CreateTable(
                name: "UserReadBooks",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReadBooks", x => new { x.UserId, x.BookId });
                });

            migrationBuilder.CreateTable(
                name: "UserReadingBooks",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReadingBooks", x => new { x.UserId, x.BookId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFavoriteBooks");

            migrationBuilder.DropTable(
                name: "UserReadBooks");

            migrationBuilder.DropTable(
                name: "UserReadingBooks");
        }
    }
}
