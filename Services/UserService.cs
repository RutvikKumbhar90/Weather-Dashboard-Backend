using Microsoft.EntityFrameworkCore;
using WeatherDashboardBackend.Data;
using WeatherDashboardBackend.Models;
using Microsoft.AspNetCore.Identity; // Add this namespace
using System.Net;
using System.Globalization;
using System.Text.Json;

namespace WeatherDashboardBackend.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<UserResponse> _passwordHasher; // Declare password hasher

        public UserService(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<UserResponse>(); // Initialize the password hasher
        }

        private string GetIndianTime()
        {
            var indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);
            return indianTime.ToString("hh:mm tt"); // AM/PM format
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "IP Not Found";
        }

        public async Task<UserResponse> CreateUserAsync(UserResponse user)
        {
            // Hash the password before saving to the database
            var hashedPassword = _passwordHasher.HashPassword(user, user.Password);

            var newUser = new UserResponse
            {
                Name = user.Name,
                Email = user.Email,
                Password = hashedPassword, // Save the hashed password
                City = user.City,
                Country = user.Country,
                Phone = user.Phone,
                PostalCode = user.PostalCode,
                CreatedAt = DateTime.Now.ToString("dd/MM/yyyy hh:mm tt", new CultureInfo("en-IN")),
                CreatedOn = GetLocalIPAddress(),
            };

            _context.User.Add(newUser);
            await _context.SaveChangesAsync();

            return newUser;
        }

        public async Task<UserResponse?> GetUserAsync(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null) return null;

            return user;
        }

        public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
        {
            var users = await _context.User.ToListAsync();
            return users;
        }

        public async Task<UserResponse?> UpdateUserAsync(int id, UserResponse user)
        {
            var existingUser = await _context.User.FindAsync(id);
            if (existingUser == null) return null;

            existingUser.Name = user.Name ?? existingUser.Name;
            existingUser.Email = user.Email ?? existingUser.Email;
            existingUser.Password = user.Password ?? existingUser.Password;
            existingUser.City = user.City ?? existingUser.City;
            existingUser.Country = user.Country ?? existingUser.Country;
            existingUser.Phone = user.Phone ?? existingUser.Phone;
            existingUser.PostalCode = user.PostalCode ?? existingUser.PostalCode;

            existingUser.UpdatedAt = DateTime.Now.ToString("dd/MM/yyyy hh:mm tt", new CultureInfo("en-IN"));
            existingUser.UpdatedOn = GetLocalIPAddress();

            await _context.SaveChangesAsync();

            return existingUser;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null) return false;

            _context.User.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserResponse?> ValidateUserAsync(string email, string password)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            // Verify the password against the stored hash
            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);

            if (result == PasswordVerificationResult.Failed)
                return null;

            user.Password = ""; // Mask password
            return user;
        }

        public string ExtractEmailErrorMessage(JsonElement errorResponse)
        {
            if (errorResponse.TryGetProperty("errors", out var errors) &&
                errors.TryGetProperty("Email", out var emailErrors) &&
                emailErrors.ValueKind == JsonValueKind.Array &&
                emailErrors.GetArrayLength() > 0)
            {
                return emailErrors[0].GetString() ?? "Unknown email error.";
            }
            return "Email error not found.";
        }
    }
}
