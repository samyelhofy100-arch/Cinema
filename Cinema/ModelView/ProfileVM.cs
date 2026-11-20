using System.ComponentModel.DataAnnotations;

namespace Cinema.ViewModels
{
    public class ProfileVM
    {
        [Required, MaxLength(100), Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone, Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        [MaxLength(250)]
        public string? Address { get; set; }

        [Display(Name = "Photo")]
        public IFormFile? ImgFile { get; set; }

        // للعرض فقط
        public string? ExistingImg { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}