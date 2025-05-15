using Microsoft.EntityFrameworkCore;
using WeatherDashboardBackend.Models;

namespace WeatherDashboardBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Make the Email field unique in the database
            modelBuilder.Entity<UserResponse>()
                .HasIndex(u => u.Email)
                .IsUnique(); // This enforces a unique constraint on the Email field
        }

        public DbSet<UserResponse> User { get; set; }
    }
}
