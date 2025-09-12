using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RollCall.Models  
{
    public class AttendanceSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; } = default!;

        [Required]
        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        public DateTime? EndTime { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        public DateTime? PlannedEndTime { get; set; }

    }
}
