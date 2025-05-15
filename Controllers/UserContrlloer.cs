using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WeatherDashboardBackend.Models;
using WeatherDashboardBackend.Services;

namespace WeatherDashboardBackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("currentuser")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token.");

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized("Invalid user ID in token.");

            var user = await _userService.GetUserAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            user.Password = null; // Remove the password from response
            return Ok(user);
        }

        [HttpPut("currentuser")]
        public async Task<ActionResult<UserResponse>> UpdateCurrentUser([FromBody] UserResponse user)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized("Invalid or missing user ID in token.");

            try
            {
                var updatedUser = await _userService.UpdateUserAsync(userId, user);
                if (updatedUser == null)
                    return NotFound("User not found.");

                return Ok(updatedUser);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("currentuser")]
        public async Task<IActionResult> DeleteCurrentUser()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized("Invalid or missing user ID in token.");

            var result = await _userService.DeleteUserAsync(userId);
            if (!result)
                return NotFound("User not found.");

            return NoContent();
        }

        [HttpPatch("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] UpdatePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest("New password must not be empty.");

            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest("Passwords do not match.");

            var success = await _userService.ResetUserPasswordAsync(request.Email, request.NewPassword);
            if (!success)
                return NotFound("User not found.");

            return NoContent();
        }
    }
}
