using Microsoft.EntityFrameworkCore;
using WeatherDashboardBackend.Data;
using WeatherDashboardBackend.Models;

namespace WeatherDashboardBackend.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Create a new user
        public async Task<UserResponse> CreateUserAsync(UserResponse user)
        {
            var newUser = new UserResponse
            {
                Name = user.Name,
                Email = user.Email,
                Password = user.Password,
                City = user.City,
                Country = user.Country,
                Phone = user.Phone,
                PostalCode = user.PostalCode
            };

            _context.User.Add(newUser);
            await _context.SaveChangesAsync();

            return newUser;
        }

        // Get user by Id
        public async Task<UserResponse?> GetUserAsync(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null) return null;

            return new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Password = user.Password,
                City = user.City,
                Country = user.Country,
                Phone = user.Phone,
                PostalCode = user.PostalCode
            };
        }

        // Get all users
        public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
        {
            var users = await _context.User.ToListAsync();
            return users.Select(user => new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Password = user.Password,
                City = user.City,
                Country = user.Country,
                Phone = user.Phone,
                PostalCode = user.PostalCode
            }).ToList();
        }

        // Update user
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

            await _context.SaveChangesAsync();

            return new UserResponse
            {
                Id = existingUser.Id,
                Name = existingUser.Name,
                Email = existingUser.Email,
                Password = existingUser.Password,
                City = existingUser.City,
                Country = existingUser.Country,
                Phone = existingUser.Phone,
                PostalCode = existingUser.PostalCode
            };
        }

        // Delete user by Id
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
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
            if (user == null) return null;

            return new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Password = "" // Mask password
            };
        }
    }
}
