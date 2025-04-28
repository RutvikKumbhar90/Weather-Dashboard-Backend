using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using WeatherDashboardBackend.Data;
using WeatherDashboardBackend.Models;

namespace WeatherDashboardBackend.Validation
{
    public class UniqueEmailAttribute : ValidationAttribute
    {
        private readonly ApplicationDbContext _context;

        public UniqueEmailAttribute(ApplicationDbContext context)
        {
            _context = context;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var email = value.ToString()?.Trim().ToLower();

            var existingUser = _context.User.FirstOrDefault(u => u.Email.ToLower() == email);
            if (existingUser != null)
            {
                return new ValidationResult("This email is already registered.");
            }

            return ValidationResult.Success;
        }
    }
}
