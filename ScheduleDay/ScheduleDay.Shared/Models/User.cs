using System.ComponentModel.DataAnnotations;

namespace ScheduleDay.Shared.Models
{
    public class User
    {
        // User Model definition
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        // One-to-many relationship with TaskItem
        public ICollection<TaskItem>? Tasks { get; set; }
    }
}
