using Microsoft.AspNetCore.Mvc;
using WeatherDashboardBackend.Models;
using WeatherDashboardBackend.Services;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json; // 🆕 Add for JsonElement

namespace WeatherDashboardBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public AuthController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<UserResponse>> Register(UserResponse user)
        {
            if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
            {
                return BadRequest("Email and password are required.");
            }

            var createdUser = await _userService.CreateUserAsync(user);
            return CreatedAtAction(nameof(Register), new { id = createdUser.Id }, createdUser);
        }


        // POST: api/Auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Email and password are required.");
            }

            var user = await _userService.ValidateUserAsync(request.Email, request.Password);
            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }

            var token = _tokenService.GenerateToken(user);

            return Ok(new LoginResponse
            {
                Token = token,
                Email = user.Email,
                Name = user.Name
            });
        }
    }
}
