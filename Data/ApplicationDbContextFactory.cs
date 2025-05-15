using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace WeatherDashboardBackend.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables() // Add env vars support
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnectionPostgreSQL");

            var envConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            if (!string.IsNullOrEmpty(envConnectionString))
            {
                connectionString = envConnectionString;
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
