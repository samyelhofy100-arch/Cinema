using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinema.Migrations
{
    /// <inheritdoc />
    public partial class AddDataToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("insert into Categories (Name) values ('Action');insert into Categories (Name) values ('Comedy');insert into Categories (Name) values ('Drama');insert into Categories (Name) values ('Horror');insert into Categories (Name) values ('Romance');insert into Categories (Name) values ('Thriller');insert into Categories (Name) values ('Sci-Fi');insert into Categories (Name) values ('Adventure');insert into Categories (Name) values ('Animation');insert into Categories (Name) values ('Fantasy');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Delete from Categories");
        }
    }
}
