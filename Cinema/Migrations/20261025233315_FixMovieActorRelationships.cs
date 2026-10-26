using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinema.Migrations
{
    /// <inheritdoc />
    public partial class FixMovieActorRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MainImg",
                table: "Movies",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "MovieActorActorId",
                table: "MovieActors",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MovieActorMovieId",
                table: "MovieActors",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovieActors_MovieActorMovieId_MovieActorActorId",
                table: "MovieActors",
                columns: new[] { "MovieActorMovieId", "MovieActorActorId" });

            migrationBuilder.AddForeignKey(
                name: "FK_MovieActors_MovieActors_MovieActorMovieId_MovieActorActorId",
                table: "MovieActors",
                columns: new[] { "MovieActorMovieId", "MovieActorActorId" },
                principalTable: "MovieActors",
                principalColumns: new[] { "MovieId", "ActorId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovieActors_MovieActors_MovieActorMovieId_MovieActorActorId",
                table: "MovieActors");

            migrationBuilder.DropIndex(
                name: "IX_MovieActors_MovieActorMovieId_MovieActorActorId",
                table: "MovieActors");

            migrationBuilder.DropColumn(
                name: "MovieActorActorId",
                table: "MovieActors");

            migrationBuilder.DropColumn(
                name: "MovieActorMovieId",
                table: "MovieActors");

            migrationBuilder.AlterColumn<string>(
                name: "MainImg",
                table: "Movies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
