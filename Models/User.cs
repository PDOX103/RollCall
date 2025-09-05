using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace RollCall.Models
{
    [Index(nameof(Email), IsUnique = true)] // Ensure unique emails
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; } // hashed password

        [Required]
        public string Role { get; set; } // "Student", "Teacher", "Admin"

        // Optional fields (nullable to avoid InvalidCastException)
        [Phone]
        public string? PhoneNumber { get; set; }

        public string? Department { get; set; }

        // Student-specific
        public string? StudentId { get; set; }
        public string? Section { get; set; }

        // Teacher-specific
        public string? Designation { get; set; }
    }
}
