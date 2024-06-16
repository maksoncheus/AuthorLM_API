using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthorLM_API.Migrations
{
    /// <inheritdoc />
    public partial class ForeignKeysForLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserReadingBooks_BookId",
                table: "UserReadingBooks",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_UserReadBooks_BookId",
                table: "UserReadBooks",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteBooks_BookId",
                table: "UserFavoriteBooks",
                column: "BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavoriteBooks_Books_BookId",
                table: "UserFavoriteBooks",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavoriteBooks_Users_UserId",
                table: "UserFavoriteBooks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_UserReadBooks_Books_BookId",
                table: "UserReadBooks",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserReadBooks_Users_UserId",
                table: "UserReadBooks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_UserReadingBooks_Books_BookId",
                table: "UserReadingBooks",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserReadingBooks_Users_UserId",
                table: "UserReadingBooks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFavoriteBooks_Books_BookId",
                table: "UserFavoriteBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavoriteBooks_Users_UserId",
                table: "UserFavoriteBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserReadBooks_Books_BookId",
                table: "UserReadBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserReadBooks_Users_UserId",
                table: "UserReadBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserReadingBooks_Books_BookId",
                table: "UserReadingBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserReadingBooks_Users_UserId",
                table: "UserReadingBooks");

            migrationBuilder.DropIndex(
                name: "IX_UserReadingBooks_BookId",
                table: "UserReadingBooks");

            migrationBuilder.DropIndex(
                name: "IX_UserReadBooks_BookId",
                table: "UserReadBooks");

            migrationBuilder.DropIndex(
                name: "IX_UserFavoriteBooks_BookId",
                table: "UserFavoriteBooks");
        }
    }
}
