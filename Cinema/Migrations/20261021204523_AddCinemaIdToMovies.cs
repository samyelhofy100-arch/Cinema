using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinema.Migrations
{
    /// <inheritdoc />
    public partial class AddCinemaIdToMovies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) أضف العمود الجديد
            migrationBuilder.AddColumn<int>(
                name: "CinemaId",
                table: "Movies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 2) انقل القيم من العمود القديم، ولو فاضي استخدم أول سينما
            migrationBuilder.Sql(
                "UPDATE Movies SET CinemaId = ISNULL(CinemasId, (SELECT TOP 1 Id FROM Cinemas))");

            // 3) بعد كده امسح القديم
            migrationBuilder.DropForeignKey(
                name: "FK_Movies_Cinemas_CinemasId",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_CinemasId",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "CinemasId",
                table: "Movies");

            // 4) اعمل الـ Index والـ FK الجداد
            migrationBuilder.CreateIndex(
                name: "IX_Movies_CinemaId",
                table: "Movies",
                column: "CinemaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Movies_Cinemas_CinemaId",
                table: "Movies",
                column: "CinemaId",
                principalTable: "Cinemas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movies_Cinemas_CinemaId",
                table: "Movies");

            migrationBuilder.DropIndex(
                name: "IX_Movies_CinemaId",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "CinemaId",
                table: "Movies");

            migrationBuilder.AddColumn<int>(
                name: "CinemasId",
                table: "Movies",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Movies_CinemasId",
                table: "Movies",
                column: "CinemasId");

            migrationBuilder.AddForeignKey(
                name: "FK_Movies_Cinemas_CinemasId",
                table: "Movies",
                column: "CinemasId",
                principalTable: "Cinemas",
                principalColumn: "Id");
        }
    }
}
