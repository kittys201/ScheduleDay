using System.ComponentModel.DataAnnotations;

namespace ScheduleDay.Shared.Models
{
    public class LoginRequest
    {
        // Login request data validation
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}