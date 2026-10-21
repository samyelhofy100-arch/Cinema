using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinema.Migrations
{
    /// <inheritdoc />
    public partial class AddDataToCinemas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("insert into Cinemas (Name, Description, Img) values ('City Stars Cinema', 'A popular cinema destination inside City Stars Mall offering a comfortable environment to enjoy the latest movies', '1.jpg');insert into Cinemas (Name, Description, Img) values ('Galaxy Cinema', 'A cinema in Cairo where movie fans can enjoy Egyptian and international films in a dedicated movie theater', '2.jpg');insert into Cinemas (Name, Description, Img) values ('Zawya Cinema', 'An independent cinema known for showcasing alternative films and independent productions', '3.jpg');insert into Cinemas (Name, Description, Img) values ('Point 90 Cinema', 'A modern cinema destination at Point 90 Mall offering movie screenings in a contemporary entertainment setting', '4.jpg');insert into Cinemas (Name, Description, Img) values ('VOX Cinemas Mall of Egypt', 'A cinema destination at Mall of Egypt featuring a range of movie experiences for visitors and film enthusiasts', '5.jpg');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Delete Form Cinemas");
        }
    }
}
