
namespace Cinema.ViewModels
{
    public class RegisterVM
    {
        [Required, MaxLength(100), Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;
        [Required, MaxLength(500), Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required, MaxLength(50), Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Display(Name = "Confirm password")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}