using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
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

        public TokenService(IConfiguration config)
        {
            _jwtSettings = config.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();

            // Check secret key length at startup
            var keyLength = Encoding.UTF8.GetByteCount(_jwtSettings.SecretKey);
            if (keyLength < 32)
            {
                throw new ArgumentException($"SecretKey must be at least 32 bytes (256 bits). Current length: {keyLength} bytes.");
            }
        }

        public string GenerateToken(UserResponse user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? ""),
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
