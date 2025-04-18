using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WeatherDashboardBackend.Models;

namespace WeatherDashboardBackend.Services
{
    public interface ITokenService
    {
        string GenerateToken(UserResponse user);
    }

    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;

        // Constructor to inject the configuration and get JwtSettings from appsettings.json
        public TokenService(IConfiguration config)
        {
            _jwtSettings = config.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();

        }

        // Method to generate JWT token for the user
        public string GenerateToken(UserResponse user)
        {
            // Define the claims to be included in the token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? ""),
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Create a key from the secret key stored in the configuration
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

            // Create signing credentials using HMAC SHA256 algorithm
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create the JWT token with the necessary claims, expiry, and signing credentials
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: creds
            );

            // Return the JWT token as a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
