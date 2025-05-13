using WeatherDashboardBackend.Models;

public interface IUserService
{
    Task<UserResponse> CreateUserAsync(UserResponse user);
    Task<UserResponse?> GetUserAsync(int id);
    Task<IEnumerable<UserResponse>> GetAllUsersAsync();
    Task<UserResponse?> UpdateUserAsync(int id, UserResponse user);
    Task<bool> DeleteUserAsync(int id);
    Task<UserResponse?> ValidateUserAsync(string email, string password);
    Task<bool> IsEmailDuplicateAsync(string email);

    // New method for resetting password using email
    Task<bool> ResetUserPasswordAsync(string email, string newPassword);
}
