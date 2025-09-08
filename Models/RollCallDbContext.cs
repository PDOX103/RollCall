using Microsoft.EntityFrameworkCore;

namespace RollCall.Models
{
    public class RollCallDbContext : DbContext
    {
        public RollCallDbContext(DbContextOptions<RollCallDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }

        public DbSet<Enrollment> Enrollments { get; set; }

        public DbSet<AttendanceSession> AttendanceSessions { get; set; }

        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Enforce unique email at DB level
            modelBuilder.Entity<User>()
                        .HasIndex(u => u.Email)
                        .IsUnique();

            // Enforce unique course code at DB level
            modelBuilder.Entity<Course>()
                        .HasIndex(c => c.Code)
                        .IsUnique();

            //Prevent duplicate enrollments
            modelBuilder.Entity<Enrollment>()
                    .HasIndex(e => new { e.StudentId, e.CourseId })
                    .IsUnique();

          

            modelBuilder.Entity<AttendanceRecord>()
               .HasIndex(r => new { r.SessionId, r.StudentId })
               .IsUnique();
            base.OnModelCreating(modelBuilder);
        }
    }
}