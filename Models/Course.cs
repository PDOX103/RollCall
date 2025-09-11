using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RollCall.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = default!; // Add = default!

        // This will be the unique code for students to enroll
        [Required]
        public string Code { get; set; } = default!; // Add = default!

        // Foreign Key to the User (Teacher) who created this course
        public int TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public User Teacher { get; set; } = default!; // Add = default!

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}