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

        [HttpPost]
        public async Task<ActionResult<RoomModel>> CreateRoom([FromBody]RoomModel roomModel, [FromQuery] string creatingUserId)
        {
            try
            {
                var createdRoom = await _roomService.CreateRoomAsync(roomModel, creatingUserId);
                return Ok(createdRoom);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{roomId}")]
        public async Task<ActionResult<RoomModel>> GetRoom(string roomId)
        {
            var room = await _roomService.GetRoomAsync(roomId);
            if (room == null)
            {
                return NotFound($"Room with ID {roomId} not found.");
            }
            return Ok(room);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomModel>>> GetAllRooms()
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            return Ok(rooms);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRoom(RoomModel roomModel)
        {
            await _roomService.UpdateRoomAsync(roomModel);
            return NoContent();
        }

        [HttpDelete("{roomId}")]
        public async Task<IActionResult> DeleteRoom(string roomId)
        {
            await _roomService.DeleteRoomAsync(roomId);
            return NoContent();
        }
    }
}
