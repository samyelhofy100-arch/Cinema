namespace Cinema.Models
{
    public class Seat
    {
        public int Id { get; set; }

        public int HallId { get; set; }
        public Hall Hall { get; set; } = null!;

        public string Row { get; set; } = string.Empty;  
        public int Number { get; set; }                 

        public string Label => Row + Number;           
    }
}