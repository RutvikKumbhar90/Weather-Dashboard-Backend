using Microsoft.AspNetCore.Mvc;
using WeatherDashboardBackend.Models;
using WeatherDashboardBackend.Services;
using Microsoft.AspNetCore.Authorization;

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

        // New method to check if an email is already taken
        [HttpGet("checkemail/{email}")]
        [AllowAnonymous]
        public async Task<ActionResult> CheckEmailDuplicate(string email)
        {
            var isDuplicate = await _userService.IsEmailDuplicateAsync(email);
            if (isDuplicate)
            {
                return Conflict(new { message = "This email is already registered." });
            }
            return Ok(new { message = "This email is available." });
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

            // Check if email is already registered
            var isDuplicate = await _userService.IsEmailDuplicateAsync(user.Email);
            if (isDuplicate)
            {
                return Conflict(new { message = "This email is already registered." });
            }

            // Proceed with user registration
            try
            {
                var createdUser = await _userService.CreateUserAsync(user);
                return CreatedAtAction(nameof(Register), new { id = createdUser.Id }, createdUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
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
