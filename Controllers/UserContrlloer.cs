using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WeatherDashboardBackend.Models;
using WeatherDashboardBackend.Services;

namespace WeatherDashboardBackend.Controllers
{
    // The UserController handles user-related actions.
    [Authorize] // ensures that only authenticated users can access these endpoints.
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        // Constructor for injecting the IUserService
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // Endpoint to get all users (accessible only by authorized users)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // Endpoint to get the current logged-in user (based on JWT token claim)
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

        // Endpoint to update the current logged-in user's information
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

        // Endpoint to delete the current logged-in user
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

        // PATCH endpoint for updating password (no login required, using email)
        [HttpPatch("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] UpdatePasswordRequest request)
        {
            // Validate the input
            if (string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest("New password must not be empty.");

            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest("Passwords do not match.");

            // Call the service to reset the password based on the email
            var success = await _userService.ResetUserPasswordAsync(request.Email, request.NewPassword);
            if (!success)
                return NotFound("User not found.");

            // Successfully reset password, no content to return
            return NoContent();
        }
    }
}
