using System.ComponentModel.DataAnnotations;
using WeatherDashboardBackend.Validation;

namespace WeatherDashboardBackend.Models
{
    public class UserResponse
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        
        public string? Email { get; set; }

        public string? Password { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Phone { get; set; }
        public string? PostalCode { get; set; }
        public string? CreatedAt { get; set; }
        public string? CreatedOn { get; set; }
        public string? UpdatedAt { get; set; }
        public string? UpdatedOn { get; set; }
    }
}
