namespace Cinema.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price {  get; set; }
        public bool Status { get; set; }
        public DateTime? DateTime {  get; set; }
        public string MainImg {  get; set; }
        public int CategoryId {  get; set; }
        public Category Category { get; set; }
        public int CinemaId { get; set; }
        public Cinemas Cinema { get; set; }
        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
        public ICollection<MovieSubImage> SubImage { get; set; } = new List<MovieSubImage>();

    }
}
