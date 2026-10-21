using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinema.Migrations
{
    /// <inheritdoc />
    public partial class AddDataToActor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("insert into Actors (name, img) values ('Tom Cruise', '1.jpg' );" +
                "insert into Actors (name, img) values ('Leonardo DiCaprio', '2.jpg' )" +
                ";insert into Actors (name, img) values ('Robert Downey Jr.', '3.jpg');" +
                "insert into Actors (name, img) values ('Dwayne Johnson', '4.jpg');" +
                "insert into Actors (name, img) values ('Brad Pitt', '5.jpg' );" +
                "insert into Actors (name, img) values ('Will Smith', '6.jpg');" +
                "insert into Actors (name, img) values ('Keanu Reeves', '7.jpg');" +
                "insert into Actors (name, img) values ('Johnny Depp', '8.jpg');" +
                "insert into Actors (name, img) values ('Chris Hemsworth', '9.jpg');" +
                "insert into Actors (name, img) values ('Ryan Reynolds', '10.jpg');" +
                "insert into Actors (name, img) values ('Morgan Freeman', '11.jpg');" +
                "insert into Actors (name, img) values ('Al Pacino', '12.jpg');" +
                "insert into Actors (name, img) values ('Robert De Niro', '13.jpg');" +
                "insert into Actors (name, img) values ('Tom Hanks', '14.jpg');" +
                "insert into Actors (name, img) values ('Christian Bale', '15.jpg');" +
                "insert into Actors (name, img) values ('Matt Damon', '16.jpg');" +
                "insert into Actors (name, img) values ('Jason Statham', '17.jpg');" +
                "insert into Actors (name, img) values ('Jackie Chan', '18.jpg');" +
                "insert into Actors (name, img) values ('Ahmed Helmy', '19.jpg');" +
                "insert into Actors (name, img) values ('Karim Abdel Aziz', '20.jpg');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Delete From Actor");

        }
    }
}
