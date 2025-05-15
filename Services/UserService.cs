using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using WeatherDashboardBackend.Data;
using WeatherDashboardBackend.Models;
using System.Net;

namespace WeatherDashboardBackend.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<UserResponse> _passwordHasher;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<UserResponse>();
        }

        public async Task<UserResponse> CreateUserAsync(UserResponse user)
        {
            user.Password = _passwordHasher.HashPassword(user, user.Password ?? "");
            user.CreatedAt = GetIndianTime();
            user.CreatedOn = GetIpAddress();

            await _context.User.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<UserResponse?> GetUserAsync(int id)
        {
            return await _context.User.FindAsync(id);
        }

        public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
        {
            return await _context.User.ToListAsync();
        }

        public async Task<UserResponse?> UpdateUserAsync(int id, UserResponse user)
        {
            var existingUser = await _context.User.FindAsync(id);
            if (existingUser == null) return null;

            // Update fields
            existingUser.Name = user.Name;
            existingUser.Email = user.Email;

            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                existingUser.Password = _passwordHasher.HashPassword(user, user.Password);
            }

            existingUser.City = user.City;
            existingUser.Country = user.Country;
            existingUser.Phone = user.Phone;
            existingUser.PostalCode = user.PostalCode;
            existingUser.UpdatedAt = GetIndianTime();
            existingUser.UpdatedOn = GetIpAddress();

            _context.User.Update(existingUser);
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

        public async Task<bool> IsEmailDuplicateAsync(string email)
        {
            return await _context.User.AnyAsync(u => u.Email == email);
        }

        public async Task<UserResponse?> ValidateUserAsync(string email, string password)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password ?? "", password);
            return result == PasswordVerificationResult.Success ? user : null;
        }

        public async Task<bool> ResetUserPasswordAsync(string email, string newPassword)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            user.Password = _passwordHasher.HashPassword(user, newPassword);
            _context.User.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        private string GetIndianTime()
        {
            var indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);
            return indiaTime.ToString("yyyy-MM-dd HH:mm:ss tt");
        }

        private string GetIpAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch
            {
                // Ignore and fallback
            }

            return "IP_NOT_FOUND";
        }
    }
}
