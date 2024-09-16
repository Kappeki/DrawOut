using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver.Core.Authentication;

namespace DrawOutApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly IUserService _userService;
        private readonly IGameService _gameService;
        public RoomController(IRoomService roomService, IUserService userService, IGameService gameService)
        {
            _roomService = roomService;
            _userService = userService;
            _gameService = gameService;
        }

        //znaci na front mora se stavi samo da treba da se unese ime sobe i da se klikne na create room
        //i moze da se postavi i password i ne mora sa checkbox 
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        [Route("CreateRoom")]
        public async Task<ActionResult> CreateRoom([FromBody] RoomRequest request)
        {
            var (userIsError, adminUser, userError) = await _userService.GetUserSessionAsync(Request);
            
            if(userIsError)
                return BadRequest(userError);
            
            if(adminUser!.Roles!.Count != 0)
                return BadRequest("You are not allowed to create a room.");

            var (roomIsError, room, roomError) = await _roomService.CreateRoomAsync(adminUser._sessionKey, request.RoomName, request.Password);
            
            if(roomIsError)
                return BadRequest(roomError);

            return Ok(new { roomUrl = room!.RoomURL });
        }
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet]
        [Route("GetAllWordPacks")]
        public async Task<ActionResult> GetAllWordPacks()
        {
            var wordPacks = await _roomService.GetAllWordPacksAsync();
            if (wordPacks == null)
            {
                return NotFound($"No word packs found.\n Error");
            }
            return Ok(wordPacks);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet]
        [Route("GetWordsByPackName")]
        public async Task<ActionResult> GetWordsByPackName([FromQuery] string packName)
        {
            var words = await _roomService.GetWordsByPackNameAsync(packName);
            if (words == null)
            {
                return NotFound($"No words found for pack {packName}.\n");
            }
            return Ok(words);
        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet]
        [Route("StartGame")]
        public async Task<ActionResult> StartGame([FromQuery] string roomId)
        {
            var (isError, room, error) = await _roomService.GetRoomByIdAsync(roomId);
            if (isError)
            {
                return NotFound($"Room with ID {roomId} not found.\n Error : {error}");
            }
            var usersInRoom = (await _roomService.GetPlayerIdsAsync(roomId)).Data;
            var gameModel = new GameModel
            {
                _id = $"game:{roomId}",
                RoomId = roomId,
                TotalRounds = usersInRoom!.Count,
                MainTimer = room!.RoundTime,
                StealTimer = room!.RoundTime / 2
            };

            var gameRoundModel = await _gameService.CreateGameAsync(gameModel, usersInRoom!);

            if(gameRoundModel == null)
            {
                return BadRequest("Error while creating game.");
            }

            return Ok("Game created, starting...");
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{roomId}/get")]
        public async Task<ActionResult<RoomModel>> GetRoom(string roomId)
        {
            var (isError, room, error) = await _roomService.GetRoomByIdAsync(roomId);
            if (isError)
            {
                return NotFound($"Room with ID {roomId} not found.\n Error : {error}");
            }
            return Ok(room);
        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("by-url/{roomURL}")]
        public async Task<ActionResult<RoomModel>> GetRoomByURL(string roomURL)
        {
            var (isError, room, error) = await _roomService.GetRoomByUrlAsync(roomURL);
            if (isError)
            {
                return NotFound($"Room with URL {roomURL} not found.\n Error : {error}");
            }
            return Ok(room);
        }

        //GET /rooms?isAscending=true&isProtected=false
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("allRooms")]
        public async Task<ActionResult> GetAllRooms([FromQuery] bool? isAscending, [FromQuery] bool? isProtected)
        {
            var (userIsError, user, userError) = await _userService.GetUserSessionAsync(Request);
            if (userIsError)
                return BadRequest(userError);
            var sessionId = user!._sessionKey;
            var (isError, rooms, error) = await _roomService.GetAllRoomsAsync(sessionId, isAscending, isProtected);
            if (isError)
            {
                return NotFound($"No rooms found.\n Error : {error}");
            }
            return Ok(rooms);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("myRooms")]
        public async Task<ActionResult> GetMyRooms()
        {
            var (userIsError, user, userError) = await _userService.GetUserSessionAsync(Request);
            if (userIsError)
                return BadRequest(userError);
            var sessionId = user!._sessionKey;
            var (isError, rooms, error) = await _roomService.GetMyRoomsAsync(sessionId);
            if (isError)
            {
                return NotFound($"No rooms found.\n Error : {error}");
            }
            return Ok(rooms);
        }

        //takodje samo za testiranje se koristi
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
            if(sessionId != roomModel.RoomAdminId) { return BadRequest("You're not allowed to change the settings of the room!"); }
            var (isError, success, error) = await _roomService.UpdateRoomAsync(roomModel);
            if(isError)
                return BadRequest($"Error while updating room : {error}");
           
            return Ok("Room updated!");
        }

        /*[HttpDelete("{roomId}/delete")]
        public async Task<IActionResult> DeleteRoom(string roomId)
        {
            await _roomService.DeleteRoomAsync(roomId);
            return NoContent();
        }*/
    }
}
