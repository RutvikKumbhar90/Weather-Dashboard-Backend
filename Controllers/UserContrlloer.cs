using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

        // 🛠️ Added: Simple error response
        public override ActionResult ValidationProblem([ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var firstError = modelStateDictionary
                .Where(kvp => kvp.Value.Errors.Count > 0)
                .Select(kvp => kvp.Value.Errors.First().ErrorMessage)
                .FirstOrDefault();

            return BadRequest(firstError ?? "Validation error occurred.");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("currentuser")]
        [Authorize]
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

            user.Password = null; // Mask password
            return Ok(user);
        }

        [HttpPut("currentuser")]
        [Authorize]
        public async Task<ActionResult<UserResponse>> UpdateCurrentUser([FromBody] UserResponse user)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized("Invalid or missing user ID in token.");

            var updatedUser = await _userService.UpdateUserAsync(userId, user);
            if (updatedUser == null)
                return NotFound("User not found.");

            return Ok(updatedUser);
        }

        [HttpDelete("currentuser")]
        [Authorize]
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
    }
}
