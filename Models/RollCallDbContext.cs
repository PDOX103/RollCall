using Microsoft.EntityFrameworkCore;

namespace RollCall.Models
{
    public class RollCallDbContext : DbContext
    {
        public RollCallDbContext(DbContextOptions<RollCallDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Enforce unique email at DB level
            modelBuilder.Entity<User>()
                        .HasIndex(u => u.Email)
                        .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
