using WeatherDashboardBackend.Models;

public interface IUserService
{
    Task<UserResponse> CreateUserAsync(UserResponse user);
    Task<UserResponse?> GetUserAsync(int id);
    Task<IEnumerable<UserResponse>> GetAllUsersAsync();
    Task<UserResponse?> UpdateUserAsync(int id, UserResponse user);
    Task<bool> DeleteUserAsync(int id);
    Task<UserResponse?> ValidateUserAsync(string email, string password);

    // Method to check if email is already in use
    Task<bool> IsEmailDuplicateAsync(string email);
}
