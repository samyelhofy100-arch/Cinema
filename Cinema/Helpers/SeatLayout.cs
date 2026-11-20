using Cinema.Models;

namespace Cinema.Helpers
{
    public static class SeatLayout
    {
        public static List<Seat> Generate(int rows, int seatsPerRow)
        {
            var seats = new List<Seat>();
            for (int r = 0; r < rows; r++)
            {
                string rowName = ((char)('A' + r)).ToString();   
                for (int n = 1; n <= seatsPerRow; n++)
                    seats.Add(new Seat { Row = rowName, Number = n });
            }
            return seats;
        }
    }
}