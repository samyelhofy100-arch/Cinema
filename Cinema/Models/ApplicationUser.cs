using Microsoft.AspNetCore.Identity;
using Cinema.Models;

    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string? Address { get; set; }
    }
