using DrawOutApp.Server.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace DrawOutApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        
        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        [Route("{roomId}/game")]
        public async Task<ActionResult> CreateGame(string roomId)
        {
            var sessionId = Request.Cookies["UserSessionId"];
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("User session is not found.");
            }
            var (isError, game, error) = await _gameService.CreateGameAsync(roomId);
            if(isError)
                return BadRequest(error);

            return Ok(game);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet]
        [Route("{gameSessionId}/game")]
        public async Task<IActionResult> GetGame(string gameSessionId)
        {
            var sessionId = Request.Cookies["UserSessionId"];
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("User session is not found.");
            }
            var (isError, game, error) = await _gameService.GetGameAsync(gameSessionId);
            if(isError)
                return BadRequest(error);

            return Ok(game);
        }



    }
}
