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

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserModel userModel)
        {
            var data = await _userService.CreateUserAsync(userModel);
            if(data.IsError)
                return BadRequest(data.Error);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("UserSessionId", data.Data!.SessionId, cookieOptions);
            return CreatedAtAction(nameof(GetUser), new { sessionId = data.Data!.SessionId }, data.Data);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("session")]
    
        public async Task<IActionResult> GetSession()
        {
            // Check if the session cookie exists
            var sessionId = Request.Cookies["UserSessionId"];
            if (string.IsNullOrEmpty(sessionId))
            {
                return NotFound("Session not found");
            }

            // Retrieve user details using the session ID
            var (isError,user,error) = await _userService.GetUserAsync(sessionId);
            if (isError)
            {
                // Consider deleting the cookie if the session doesn't exist in the backend
                Response.Cookies.Delete("UserSessionId");
                return NotFound(error);
            }

            // Return the user details
            return Ok(user);
        }
        
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{sessionId}")]
        public async Task<IActionResult> GetUser(string sessionId)
        {
          
            var (isError, user, error) = await _userService.GetUserAsync(sessionId);
            if (isError)
            {
                return NotFound(error);
            }
            return Ok(user);
        }

        //videcemo kako brisanje
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{sessionId}")]
    
        public async Task<IActionResult> DeleteUser(string sessionId)
        {
            //ostao try catch jer nema sta da vrati ako je sve ok
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

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("{sessionId}")]
        public async Task<IActionResult> UpdateUser(string sessionId, [FromBody] UserModel userModel)
        {
            var (isError, success, error) = await _userService.UpdateUserAsync(sessionId, userModel);
            if (isError)
            {
                return BadRequest(error);
            }
            //dodati da vraca sessionId za testing
            return Ok($"User successfully updated!");
        }
    }
}

