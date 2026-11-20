using System.ComponentModel.DataAnnotations;
using Cinema.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Cinema.Areas.Admin.ViewModels
{
    public class HallFormVM
    {
        public int Id { get; set; }

        [Required, MaxLength(100), Display(Name = "Hall name")]
        public string Name { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Please select a cinema.")]
        [Display(Name = "Cinema")]
        public int CinemaId { get; set; }

        [Range(1, 26, ErrorMessage = "Rows must be between 1 and 26.")]
        public int Rows { get; set; } = 8;

        [Range(1, 30, ErrorMessage = "Seats per row must be between 1 and 30.")]
        [Display(Name = "Seats per row")]
        public int SeatsPerRow { get; set; } = 12;

        [ValidateNever] public List<Cinemas> Cinemas { get; set; } = new();
        [ValidateNever] public string? CinemaName { get; set; }
    }
}