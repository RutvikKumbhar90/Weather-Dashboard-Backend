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
        }

        public DbSet<UserResponse> User { get; set; }
    }
}
