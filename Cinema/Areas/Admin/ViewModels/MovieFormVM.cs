using System.ComponentModel.DataAnnotations;
using Cinema.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Cinema.Areas.Admin.ViewModels
{
    public class MovieFormVM
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Range(0, 100000, ErrorMessage = "Enter a valid price.")]
        public decimal Price { get; set; }

        public bool Status { get; set; } = true;

        [Display(Name = "Show date")]
        public DateTime? ShowDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a cinema.")]
        [Display(Name = "Cinema")]
        public int CinemaId { get; set; }

        // Actors chosen in the form
        public List<int> SelectedActorIds { get; set; } = new();

        // Uploads
        [Display(Name = "Main image")]
        public IFormFile? MainImgFile { get; set; }
        public List<IFormFile> SubImgFiles { get; set; } = new();

        // Existing sub images the admin ticked to remove
        public List<int> RemoveSubImageIds { get; set; } = new();

        // ---- Display only (never posted) ----
        [ValidateNever] public string? ExistingMainImg { get; set; }
        [ValidateNever] public List<MovieSubImage> ExistingSubImages { get; set; } = new();
        [ValidateNever] public List<Category> Categories { get; set; } = new();
        [ValidateNever] public List<Cinemas> Cinemas { get; set; } = new();
        [ValidateNever] public List<Actor> Actors { get; set; } = new();
    }
}
