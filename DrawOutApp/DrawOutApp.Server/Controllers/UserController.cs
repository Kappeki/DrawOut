using Microsoft.AspNetCore.Mvc;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Services;
using DrawOutApp.Server.Services.Contracts;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] UserPreferences userPrefs)
        {
            var data = await _userService.CreateUserSessionAsync(userPrefs);
            if(data.IsError)
                return BadRequest(data.Error);
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                SameSite = SameSiteMode.None
            };
            Response.Cookies.Append("UserSessionId", data.Data!._sessionKey, cookieOptions);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, data.Data!._sessionKey)
            };
            var identity = new ClaimsIdentity(claims, "User");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return Ok(data.Data!);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("GetSession")]
        public async Task<IActionResult> GetSession()
        {
            var sessionKey = Request.Cookies["UserSessionId"];
            if (string.IsNullOrEmpty(sessionKey))
            {
                return NotFound("Session not found");
            }

            var (isError,user,error) = await _userService.GetUserAsync(sessionKey);
            if (isError)
            {
                Response.Cookies.Delete("UserSessionId");
                return NotFound(error);
            }

            if (!User.HasClaim(c => c.Type == "SessionId"))
            {
                // Add the session ID as a claim
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, sessionKey)
                };
                var identity = new ClaimsIdentity(claims, "UserSession");
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            }


            return Ok(user);
        }
        
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("GetUser/{sessionId}")]
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
        [HttpDelete("DeleteUser/{sessionId}")]
    
        public async Task<IActionResult> DeleteUser(string sessionId)
        {
            //ostao try catch jer nema sta da vrati ako je sve ok
            try
            {
                await _userService.DeleteUserAsync(sessionId);
                Response.Cookies.Delete("UserSessionId");
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UserPreferences userModel)
        {

            var sessionKey = Request.Cookies["UserSessionId"];
            if (string.IsNullOrEmpty(sessionKey))
            {
                return NotFound("Session not found");
            }

            var (isError, success, error) = await _userService.UpdateUserPrefsAsync(sessionKey, userModel);
            if (isError)
            {
                return BadRequest(error);
            }
            //dodati da vraca sessionId za testing
            return Ok($"User successfully updated!");
        }
    }
}

