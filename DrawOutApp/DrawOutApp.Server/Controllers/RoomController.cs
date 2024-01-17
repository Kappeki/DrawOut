using DrawOutApp.Server.Models;
using DrawOutApp.Server.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace DrawOutApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        //znaci na front mora se stavi samo da treba da se unese ime sobe i da se klikne na create room
        //i moze da se postavi i password i ne mora
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        [Route("CreateRoom")]
        public async Task<ActionResult> CreateRoom([FromBody]RoomModel roomModel)
        {
            var sessionId = Request.Cookies["UserSessionId"];
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("User session is not found.");
            }
            var (isError, room, error) = await _roomService.CreateRoomAsync(roomModel, sessionId);
            if(isError)
                return BadRequest(error);

            //ZA TESTIRANJE
            //return Ok($"Successfully created new room with name : {room.RoomName}");
            return CreatedAtAction(nameof(GetRoom), new { roomId = room.RoomId }, room);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("{roomId}/join")]
        public async Task<IActionResult> JoinRoom(string roomId)
        {
            var sessionId = Request.Cookies["UserSessionId"];
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("User session is not found.");
            }
            var (isError, nickname, error) = await _roomService.AddPlayer(roomId, sessionId);
            if(isError)
                return BadRequest(error);
            return Ok($"{nickname} joined the room!");
        }
        //korisnik SAM izlazi iz sobe, negde drugde mora kad bi korisnik bio kickovan
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("{roomId}/leave")]
        public async Task<IActionResult> LeaveRoom(string roomId)
        {
            var sessionId = Request.Cookies["UserSessionId"];
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("User session is not found.");
            }
            var (isError, nickname, error) = await _roomService.RemovePlayer(roomId, sessionId);
            if (isError)
            {
                return BadRequest(error);
            }
            return Ok($"{nickname} left the room!");
          
        }
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{roomId}/get")]
        public async Task<ActionResult<RoomModel>> GetRoom(string roomId)
        {
            var (isError, room, error) = await _roomService.GetRoomAsync(roomId);
            if (isError)
            {
                return NotFound($"Room with ID {roomId} not found.\n Error : {error}");
            }
            return Ok(room);
        }
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("rooms")]

        public async Task<ActionResult> GetAllRooms()
        {
            var (isError,rooms, error) = await _roomService.GetAllRoomsAsync();
            if (isError)
            {
                return NotFound($"No rooms found.\n Error : {error}");
            }
            return Ok(rooms);
        }
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateRoom(RoomModel roomModel)
        {
            var sessionId = Request.Cookies["UserSessionId"];
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("User session is not found.");
            }
            if(sessionId != roomModel.RoomAdmin!.SessionId) { return BadRequest("You're not allowed to change the settings of the room!"); }
            var (isError, success, error) = await _roomService.UpdateRoomAsync(roomModel);
            if(isError)
                return BadRequest($"Error while updating room : {error}");
            return Ok("Room updated!");
        }

        [HttpDelete("{roomId}/delete")]
        public async Task<IActionResult> DeleteRoom(string roomId)
        {
            await _roomService.DeleteRoomAsync(roomId);
            return NoContent();
        }
    }
}
