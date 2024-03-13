using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Mappers;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Repositories;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Services.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;


namespace DrawOutApp.Server 
{
    public record class Username(string Value);
}


namespace DrawOutApp.Server.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepo _roomRepository;
        private readonly IUserService _userService;
        private readonly PasswordHasher<Room> _passwordHasher = new();
 
        public RoomService(IRoomRepo roomRepository, IUserService userService)
        {
            _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository)); 
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<Result<RoomModel,string>> CreateRoomAsync(string creatingUserId, string roomName, string? password = null)
        {
            try
            {
                var adminResult = await _userService.GetUserAsync(creatingUserId);
                if (adminResult.IsError)
                {
                    return $"Creating user not found, error : {adminResult.Error}";
                }

                var adminModel = adminResult.Data!;
                var roomModel = new RoomModel
                {
                    RoomName = roomName,
                    RoomAdmin = adminModel,
                    Players = new List<UserModel> { adminModel },
                    PlayerCount = 1,
                    GameState = GameState.Waiting,
                    RoomURL = RoomMapper.GenerateRoomURL(roomName)
                };

                var roomEntity = RoomMapper.ToEntity(roomModel);

                if(password != null)
                {
                    roomEntity.Password = _passwordHasher.HashPassword(roomEntity, password);
                }
               
                var room = await _roomRepository.CreateRoomAsync(roomEntity);

                await _userService.AddRole(room.ObjectId, creatingUserId, Role.RoomAdmin);

                return RoomMapper.ToModel(room);
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error creating room. : {error}";
            }
        }

        public async Task<Result<Username,string>> AddPlayerAsync(string roomId, string sessionId, string? password = null)
        { 
            using var session = _roomRepository.GetSession();
            session.StartTransaction();
            try
            {
                var roomResult = await GetRoomByIdAsync(roomId);
                var userResult = await _userService.GetUserAsync(sessionId);
                
                if (roomResult.IsError)
                {
                    return "Room not found. : " + roomResult.Error;
                }
                if (userResult.IsError)
                {
                    return $"User not found. : {userResult.Error}";
                }

                var roomModel = roomResult.Data!;
                var userModel = userResult.Data!;

                if(userModel.SeshKey == roomModel.RoomAdmin.SeshKey)
                {
                    await _userService.AddRole(roomId, userModel.SeshKey, Role.RoomAdmin);
                }

                //mozda da stoji direktno u controller nmp 
                if (roomModel.PasswordHash != null)
                {
                    if (password == null)
                    {
                        return "Room is password protected.";
                    }
                    if (!_passwordHasher.VerifyHashedPassword(RoomMapper.ToEntity(roomModel), roomModel.PasswordHash, password)
                                        .Equals(PasswordVerificationResult.Success))
                    {
                        return "Invalid password.";
                    }
                }

                if (roomModel.PlayerCount == 8)
                {
                    return "Room is full!";
                }
                else
                { 
                    roomModel.PlayerCount++;
                    roomModel.Players?.Add(userModel);

                    await _roomRepository.UpdateRoomAsync(r=>r._id == ObjectId.Parse(roomId), 
                        Builders<Room>.Update.Set(r=>r.PlayerCount, roomModel.PlayerCount),
                        session);

                    await _roomRepository.InsertIntoListAsync(
                        r => r._id == ObjectId.Parse(roomId),
                        r => r.Players!,
                        UserMapper.ToEntity(userModel),
                        session);

                    await session.CommitTransactionAsync();

                    return new Username(userModel.Nickname!);
                }
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                string error = ErrorHandler.HandleError(ex);
                return $"Error joining room. : {error}";
            }
            finally
            {
                session.Dispose();
            }
        }
       
        public async Task<Result<Username,string>> RemovePlayerAsync(string roomId, string sessionId)
        {
            try
            {
                var roomResult = await GetRoomByIdAsync(roomId);
                var userResult = await _userService.GetUserAsync(sessionId);

                if (roomResult.IsError)
                {
                    return "Room not found. : " + roomResult.Error;
                }
                if (userResult.IsError)
                {
                    return $"User not found. : {userResult.Error}";
                }

                var roomModel = roomResult.Data!;
                var userModel = userResult.Data!;

                if (userModel.SeshKey == roomModel.RoomAdmin.SeshKey)
                {
                    await _userService.RemoveRole(roomId, userModel.SeshKey, Role.RoomAdmin);
                }

                var filter = Builders<Room>.Filter.Eq(r => r._id, ObjectId.Parse(roomId));
                await _roomRepository.RemoveFromListAsync(
                    filter, 
                    r => r.Players!, 
                    u => u.Nickname == userModel.Nickname);

                return new Username(userModel.Nickname!);
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error removing player from room. : {error}";
            }
        }

        public async Task<Result<RoomModel?, string>> GetRoomByUrlAsync(string roomUrl)
        {
            RoomModel roomModel = default!;
            try
            {
                var roomEntity = await _roomRepository.GetRoomByFilterAsync(r => r.RoomURL == roomUrl);
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
                return $"Error getting room by url. : {error}";
            }
        }

        public async Task<Result<RoomModel?,string>> GetRoomByIdAsync(string roomId)
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

        public async Task<Result<List<RoomModel>, string>> GetAllRoomsAsync(string sessionId, bool? isAscending = null, bool? isProtected = null)
        {
            List<RoomModel> roomList = new List<RoomModel>();
            try
            {
                var currentUserResult = await _userService.GetUserAsync(sessionId);
                if(currentUserResult.IsError)
                {
                    return $"User not found. Session error : {currentUserResult.Error}";
                }
                var currentUser = currentUserResult.Data!;
                
                var baseFilter = Builders<Room>.Filter
                    .And(
                    Builders<Room>.Filter.Eq(r => r.GameState, GameState.Waiting), 
                    Builders<Room>.Filter.Ne(r=>r.RoomAdmin!.ObjectId, currentUser.MongoId)
                    );


                if (isProtected != null)
                {
                    if (isProtected == true)
                    {
                        baseFilter &= Builders<Room>.Filter.Ne(r => r.Password, null);
                    }
                    else
                    {
                        baseFilter &= Builders<Room>.Filter.Eq(r => r.Password, null);
                    }
                }
                
                SortDefinition<Room>? sort = null;
                
                if (isAscending != null)
                {
                    sort = isAscending == true ? Builders<Room>.Sort.Ascending(r => r.PlayerCount) : Builders<Room>.Sort.Descending(r => r.PlayerCount);
                }

                var roomEntities = await _roomRepository.GetAllRoomsAsync(baseFilter,sort);

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

        public async Task<Result<List<RoomModel>, string>> GetMyRoomsAsync(string sessionId)
        {
            List<RoomModel> roomList = new List<RoomModel>();
            try
            {
                var currentUserResult = await _userService.GetUserAsync(sessionId);
                if(currentUserResult.IsError)
                {
                    return $"User not found. Session error : {currentUserResult.Error}";
                }
                var currentUser = currentUserResult.Data!;

                var filter = Builders<Room>.Filter.Eq(r => r.RoomAdmin!.ObjectId, currentUser.MongoId);

                var roomEntities = await _roomRepository.GetAllRoomsAsync(filter);

                roomList = roomEntities.Select(RoomMapper.ToModel).ToList();

                if (roomList.Count == 0)
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
                var filter  = Builders<Room>.Filter.Eq(r => r.RoomURL, roomModel.RoomURL);
                var roomEntity = RoomMapper.ToEntity(roomModel);
                await _roomRepository.UpdateRoomAsync(filter, Builders<Room>.Update.Set(r => r, roomEntity)); 
                return true;
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error updating room. : {error}";
            }
        }

        /*public async Task<Result<bool,string>> UpdateRoundTimerAsync(string roomId, RoundTime roundTime)
        {
            try
            {
                var filter = Builders<Room>.Filter.Eq(r => r._id, ObjectId.Parse(roomId));
                await _roomRepository.UpdateRoomAsync(filter, Builders<Room>.Update.Set(r => r.RoundTime, roundTime));
                return true;
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error updating round timer. : {error}";
            }
        }

        public async Task UpdateGameStateAsync(string roomId, GameState gameState)
        {
            var filter = Builders<Room>.Filter.Eq(r => r._id, ObjectId.Parse(roomId));
            await _roomRepository.UpdateRoomAsync(filter, Builders<Room>.Update.Set(r => r.GameState, gameState));
        }*/

        public async Task DeleteRoomAsync(string roomId)
        {
            await _roomRepository.DeleteRoomAsync(roomId);
        }

    }
}
