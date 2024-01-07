using Microsoft.AspNetCore.Mvc;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Services;
using DrawOutApp.Server.Services.Contracts;

namespace DrawOutApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserModel userModel)
        {
            if (userModel == null)
            {
                return BadRequest("User data is required");
            }
            try
            {

                var createdUser = await _userService.CreateUserAsync(userModel);
                return CreatedAtAction(nameof(GetUser), new { sessionId = createdUser.SessionId }, createdUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("{sessionId}")]
        public async Task<IActionResult> GetUser(string sessionId)
        {
            try
            {
                var user = await _userService.GetUserAsync(sessionId);
                if (user == null)
                {
                    return NotFound();
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpDelete("{sessionId}")]
        public async Task<IActionResult> DeleteUser(string sessionId)
        {
            try
            {
                await _userService.DeleteUserAsync(sessionId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("{sessionId}")]
        public async Task<IActionResult> UpdateUser(string sessionId, [FromBody] UserModel userModel)
        {
            if (userModel == null || sessionId != userModel.SessionId)
            {
                return BadRequest("Invalid user data");
            }

            try
            {
                await _userService.UpdateUserAsync(sessionId, userModel);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}

