using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Cinema.Areas.Admin.ViewModels
{
    public class ShowtimeCreateVM
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select a movie.")]
        [Display(Name = "Movie")]
        public int MovieId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a hall.")]
        [Display(Name = "Hall")]
        public int HallId { get; set; }

        [Required(ErrorMessage = "Please choose the first day.")]
        [DataType(DataType.Date), Display(Name = "First day")]
        public DateTime? StartDate { get; set; }

        [Range(1, 30, ErrorMessage = "Days must be between 1 and 30.")]
        [Display(Name = "Number of days")]
        public int Days { get; set; } = 1;

        [Required(ErrorMessage = "Enter at least one time, e.g. 14:00, 17:30")]
        public string Times { get; set; } = string.Empty;

        [Range(1, 100000, ErrorMessage = "Enter a valid price.")]
        [Display(Name = "Ticket price (EGP)")]
        public decimal Price { get; set; }

        [ValidateNever] public List<Movie> Movies { get; set; } = new();
        [ValidateNever] public List<Hall> Halls { get; set; } = new();
    }
}