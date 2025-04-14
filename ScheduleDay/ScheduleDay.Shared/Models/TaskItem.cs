using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScheduleDay.Shared.Models
{
    public class TaskItem
    {
        // TaskItem Model definition
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set => _date = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        public string Status { get; set; } = "Pending";
        [NotMapped]
        public bool? GoogleEvent { get; set; } = false;

        [ForeignKey("User")]
        public int UserID { get; set; }
        public User? User { get; set; }

        public TaskItem()
        {
            _date = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
        }
    }
}