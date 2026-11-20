using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Cinema.Areas.Admin.ViewModels
{
    public class ShowtimeEditVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please choose the start time.")]
        [Display(Name = "Start time")]
        public DateTime? StartTime { get; set; }

        [Range(1, 100000, ErrorMessage = "Enter a valid price.")]
        [Display(Name = "Ticket price (EGP)")]
        public decimal Price { get; set; }

        [ValidateNever] public string MovieName { get; set; } = string.Empty;
        [ValidateNever] public string HallName { get; set; } = string.Empty;
    }
}