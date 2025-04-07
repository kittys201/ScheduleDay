using System.ComponentModel.DataAnnotations;

namespace ScheduleDay.Shared.Models
{
    public class User
    {
        // User Model definition
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;

        // One-to-many relationship with TaskItem
        public ICollection<TaskItem>? Tasks { get; set; }
    }
}
