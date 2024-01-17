using DrawOutApp.Server.Mappers;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Repositories;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Services.Contracts;


namespace DrawOutApp.Server 
{
    public class Username
    {
        public string? Value { get; set; }
    }
}


namespace DrawOutApp.Server.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepo _roomRepository;
        private readonly IUserService _userService;
        private readonly IGameService _gameService;
        public RoomService(IRoomRepo roomRepository, IUserService userService, ITeamService teamService, IGameService gameService)
        {
            _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository)); 
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
        }

        public async Task<Result<RoomModel,string>> CreateRoomAsync(RoomModel roomModel, string creatingUserId)
        {
            try
            {
                var adminUser = await _userService.GetUserAsync(creatingUserId);
                if (adminUser.IsError)
                {
                    return $"Creating user not found, error : {adminUser.Error}";
                }

                roomModel.RoomAdmin = adminUser.Data;
                if (!adminUser.Data!.Roles.Contains(Role.RoomAdmin))
                {
                    await _userService.AddRole(creatingUserId, Role.RoomAdmin);
                }

                roomModel.Players?.Add(adminUser.Data);
                roomModel.PlayerCount++;
                roomModel.GameState = GameState.Waiting;
                var roomEntity = RoomMapper.ToEntity(roomModel);
                var room = await _roomRepository.CreateRoomAsync(roomEntity);
                //kreira se igra i timovi, ne inicijalizuje se
                var (isError, data, error) = await _gameService.CreateGameAsync(room.RoomId);
                if (isError)
                    return $"Error occured while creating game : {error}";
               
                return RoomMapper.ToModel(roomEntity);
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error creating room. : {error}";
            }
        }

        public async Task<Result<Username,string>> AddPlayer(string roomId, string sessionId)
        {
            Username username = default!;
            try
            {
                var roomModel = await GetRoomAsync(roomId);
                var userModel = await _userService.GetUserAsync(sessionId);
                if (roomModel.IsError)
                {
                    return "Room not found. : " + roomModel.Error;
                }
                if (userModel.IsError)
                {
                    return $"User not found. : {userModel.Error}";
                }

                if (roomModel.Data!.PlayerCount == 8)
                {
                    return "Room is full!";
                }
                else
                {
                    roomModel.Data.PlayerCount++;
                    roomModel.Data.Players?.Add(userModel.Data!);
                    //await _userService.AddRole(sessionId, Role.Player);
                    username.Value = userModel.Data!.Nickname;
                    var (isError, success, error) = await UpdateRoomAsync(roomModel.Data!);
                    if(!success)
                        return $"Error updating room. : {error}";

                }
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error joining room. : {error}";
            }
            return username;
        }

        public async Task<Result<RoomModel?,string>> GetRoomAsync(string roomId)
        {
            RoomModel roomModel = default!;
            try
            {
                var roomEntity = await _roomRepository.GetRoomAsync(roomId);
                if (roomEntity == null)
                {
                    return "Room not found.";
                }
                
                roomModel = RoomMapper.ToModel(roomEntity);
                return roomModel;
             
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error getting room. : {error}";
            }
        }

        public async Task<Result<List<RoomModel>,string>> GetAllRoomsAsync()
        {
            List<RoomModel> roomList = new List<RoomModel>();
            try
            {
                var roomEntities = await _roomRepository.GetAllRoomsAsync();
                roomList = roomEntities.Select(RoomMapper.ToModel).ToList();
                if(roomList.Count == 0)
                {
                    return "No rooms found.";
                }
                else
                {
                    return roomList;
                }
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error getting all rooms. : {error}";
            }
        }

        public async Task<Result<bool,string>> UpdateRoomAsync(RoomModel roomModel)
        {
            try
            {
                var roomEntity = RoomMapper.ToEntity(roomModel);
                var success = await _roomRepository.UpdateRoomAsync(roomEntity);
                if (success) return true;
                return "Couldn't update room";
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error updating room. : {error}";
            }
        }

        public async Task DeleteRoomAsync(string roomId)
        {
            await _roomRepository.DeleteRoomAsync(roomId);
        }

        //mozda bi trebalo task<string> da vraca ime igraca za notifikacije 
        public async Task<Result<Username,string>> RemovePlayer(string roomId, string sessionId)
        {
            Username username = default!;
            try
            {
                var roomModel = await GetRoomAsync(roomId);
                var userModel = await _userService.GetUserAsync(sessionId);

                if (roomModel.IsError)
                {
                    return $"Room not found : {roomModel.Error}";
                }
                if (userModel.IsError)
                {
                    return $"User not found : {userModel.Error}";
                }

                if (roomModel.Data!.Players!.Contains(userModel.Data!))
                {
                    roomModel.Data.Players.Remove(userModel.Data!);
                    roomModel.Data.PlayerCount--;
                    username.Value = userModel.Data!.Nickname;
                    //await _userService.RemoveRole(sessionId, Role.Player);
                    var success = await UpdateRoomAsync(roomModel.Data!);
                    if (success.Data)
                        return username;
                    else
                        return "Error removing player from room!";
                }
                else
                {
                    //ne bi trebalo nikad da se izvrsava
                    return "User not in room.";
                }
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error removing player from room. : {error}";
            }
        }
    }
}
