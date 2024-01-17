using DrawOutApp.Server.Models;
using DrawOutApp.Server.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace DrawOutApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;
        public TeamController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("{teamId}/add")]
     
        public async Task<IActionResult> AddTeammate(string teamId)
        {
            var sessionId = Request.Cookies["UserSessionId"];
            if(string.IsNullOrEmpty(sessionId))
            {
                return NotFound("Session not found");
            }
            var data = await _teamService.AddTeammateAsync(teamId, sessionId);
            if(data.IsError)
                return BadRequest($"Could not join team : {data.Error}");
            //mozda da dodamo nekako da se vidi kom timu se pridruzuje?
            return Ok($"You've joined the team! {teamId}");
        }
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("{teamId}/remove")]
        //za testiranje videcemo kako i dal ce ovo da se zove
        public async Task<IActionResult> RemoveTeammate(string teamId, [FromBody] string teammateId)
        {
            await _teamService.RemoveTeammateAsync(teamId, teammateId);
            return Ok($"Teammate removed from team {teamId} successfully.");
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{teamId}")]
        //mozda redundantno nzm de bi se koristilo, vise za testiranje
        public async Task<IActionResult> GetTeam(string teamId)
        {
            var (isError, team, error) = await _teamService.GetTeamAsync(teamId);

            if (isError)
                return NotFound($"Team with ID {teamId} not found. \n Error : {error}");

            return Ok(team);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("{teamId}/score")]
    
        public async Task<IActionResult> UpdateTeamScore(string teamId, [FromBody] int score)
        {
            var data = await _teamService.UpdateTeamScoreAsync(teamId, score);
            if(data.IsError)
                return NotFound($"Team with ID {teamId} not found. \n Error : {data.Error}");
            return Ok($"Team {teamId} score updated successfully.");
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("{teamId}/leader")]
     
        public async Task<IActionResult> SetTeamLeader(string teamId)
        {
            var sessionId = Request.Cookies["UserSessionId"];
            if (string.IsNullOrEmpty(sessionId))
            {
                return NotFound("Session not found");
            }
            var data = await _teamService.SetTeamLeaderAsync(teamId, sessionId);
            if(data.IsError)
                return NotFound($"Team with ID {teamId} not found. \n Error : {data.Error}");
            return Ok($"You are now the team leader!");
        }
    }
}
