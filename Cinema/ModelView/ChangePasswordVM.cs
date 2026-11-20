// ViewModels/ChangePasswordVM.cs
using System.ComponentModel.DataAnnotations;

namespace Cinema.ViewModels
{
    public class ChangePasswordVM
    {
        [Required] public string CurrentPassword { get; set; } = string.Empty;
        [Required] public string NewPassword { get; set; } = string.Empty;
        [Required, Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}