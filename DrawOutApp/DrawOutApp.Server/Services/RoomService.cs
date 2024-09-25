using AutoMapper;
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
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace DrawOutApp.Server.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepo _roomRepository;
        private readonly PasswordHasher<Room> _passwordHasher = new();
        private readonly IMapper _mapper;

        public RoomService(IRoomRepo roomRepository, IMapper mapper)
        {
            _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));
            
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<Result<RoomModel,string>> CreateRoomAsync(string creatingUserId, string roomName, string? password = null)
        {
            try
            {
                var room = new Room
                {
                    RoomName = roomName,
                    RoomAdminId = creatingUserId,
                    PlayerCount = 0,
                    RoomState = RoomState.Waiting,
                    RoomURL = GenerateRoomURL(roomName),
                    RoundTime = RoundTime.Medium
                };

                if (password != null)
                {
                    room.Password = _passwordHasher.HashPassword(room, password);
                }

                await _roomRepository.AddPlayerToSetAsync(room._id.ToString(), creatingUserId);

                room = await _roomRepository.CreateRoomAsync(room);
                return _mapper.Map<RoomModel>(room);
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error creating room. : {error}";
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
                return _mapper.Map<RoomModel>(roomEntity);
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error getting room by url. : {error}";
            }
        }
        public async Task<string?> GetIdFromURL(string roomURL)
        {
            try
            {
                var room = await _roomRepository.GetRoomByFilterAsync(r => r.RoomURL == roomURL);
                if (room == null)
                {
                    return null;
                }
                return room.ObjectId;
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return null;
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
                var playerList = await _roomRepository.GetPlayerSetAsync(roomId);
                
                roomModel = _mapper.Map<RoomModel>(roomEntity);
                roomModel.Players = playerList;

                return roomModel;
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error getting room. : {error}";
            }
        }
        public async Task<Result<List<RoomListItem>?, string>> GetAllRoomsAsync(string sessionId, bool? isAscending = null, bool? isProtected = null)
        {
            List<RoomListItem> roomList = new List<RoomListItem>();
            try
            {
                var baseFilter = Builders<Room>.Filter
                    .And(
                    Builders<Room>.Filter.Eq(r => r.RoomState, RoomState.Waiting), 
                    Builders<Room>.Filter.Ne(r=>r.RoomAdminId, sessionId)
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

                roomList = _mapper.Map<List<RoomListItem>>(roomEntities);
                
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
        public async Task<Result<List<RoomListItem>?, string>> GetMyRoomsAsync(string sessionId, bool? isAscending = null, bool? isProtected = null)
        {
            List<RoomListItem> roomList = [];
            try
            {
                var filter = Builders<Room>.Filter.Eq(r => r.RoomAdminId, sessionId);

                if (isProtected != null)
                {
                    if (isProtected == true)
                    {
                        filter &= Builders<Room>.Filter.Ne(r => r.Password, null);
                    }
                    else
                    {
                        filter &= Builders<Room>.Filter.Eq(r => r.Password, null);
                    }
                }

                SortDefinition<Room>? sort = null;

                if (isAscending != null)
                {
                    sort = isAscending == true ? Builders<Room>.Sort.Ascending(r => r.PlayerCount) : Builders<Room>.Sort.Descending(r => r.PlayerCount);
                }

                var roomEntities = await _roomRepository.GetAllRoomsAsync(filter, sort);

                roomList = _mapper.Map<List<RoomListItem>>(roomEntities);

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
                var roomEntity = await _roomRepository.GetRoomByFilterAsync(r => r.RoomURL == roomModel.RoomURL);

                if(roomEntity == null)
                {
                    return "Room not found.";
                }

                var update = Builders<Room>.Update;
                var updates = new List<UpdateDefinition<Room>>();

                if (roomModel.CustomWords != null && roomModel.CustomWords.Count != 0)
                {
                    if (roomEntity.CustomWords == null)
                    {
                        updates.Add(update.Set(r => r.CustomWords, roomModel.CustomWords));
                    }
                    else if (!roomModel.CustomWords.SequenceEqual(roomEntity.CustomWords!))
                    {
                        updates.Add(update.Set(r => r.CustomWords, roomModel.CustomWords));
                    }
                }
                if (roomModel.SelectedWordPack != null)
                {
                    if (roomModel.SelectedWordPack != roomEntity.SelectedWordPack)
                        updates.Add(update.Set(r => r.SelectedWordPack, roomModel.SelectedWordPack));

                }
                if (roomModel.RoundTime != (int)roomEntity.RoundTime)
                {
                    updates.Add(update.Set(r => r.RoundTime, (RoundTime)roomModel.RoundTime));
                }

                if (updates.Count > 0)
                {
                    var combinedUpdate = update.Combine(updates);
                    await _roomRepository.UpdateRoomAsync(filter, combinedUpdate);
                }

                return true;
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error updating room. : {error}";
            }
        }
        public async Task UpdateRoomStateAsync(string roomId, RoomState roomState)
        {
            var filter = Builders<Room>.Filter.Eq(r => r._id, ObjectId.Parse(roomId));
            var update = Builders<Room>.Update.Set(r => r.RoomState, roomState);
            await _roomRepository.UpdateRoomAsync(filter, update);
        }
        public async Task<bool> OnAdminDisconnectedAsync(string roomId, string newAdminId)
        {
            var filter = Builders<Room>.Filter.Eq(r => r._id, ObjectId.Parse(roomId));
            var update = Builders<Room>.Update.Set(r => r.RoomAdminId, newAdminId);
            await _roomRepository.UpdateRoomAsync(filter, update);
            return true;
        }
        public async Task<bool> CheckPasswordProtection(string roomId)
        {
            var room = await _roomRepository.GetRoomAsync(roomId);
            if (room == null)
            {
                return false;
            }
            if (room.Password != null)
            {
                return true;
            }
            return false;
        }
        public async Task DeleteRoomAsync(string roomId)
        {
            await _roomRepository.DeleteRoomAsync(roomId);
        }

        //pomocne funkcije u okviru service klase 
        private string GenerateRoomURL(string roomName)
        {
            var sanitizedRoomName = Regex.Replace(roomName.ToLower(), @"[^a-z0-9]", "-");
            var uniquePart = Guid.NewGuid().ToString().Substring(0, 8); 
            return $"{sanitizedRoomName}-{uniquePart}";
        }
        public async Task<Result<bool,string>> AddPlayerAsync(string roomId, string sessionId, string? password = null, bool urlJoin = false)
        { 
            try
            {
                var room = await _roomRepository.GetRoomAsync(roomId);
                if (room!.Password != null && !urlJoin)
                {
                    if (password == null)
                    {
                        return "Invalid password.";
                    }
                    if (!_passwordHasher.VerifyHashedPassword(room, room.Password, password)
                                        .Equals(PasswordVerificationResult.Success))
                    {
                        return "Invalid password.";
                    }
                }

                if (room.PlayerCount == 8)
                {
                    return "Room is full!";
                }
                else
                { 
                    await _roomRepository.AddPlayerToSetAsync(roomId, sessionId);
                    return true;
                }
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error joining room. : {error}";
            }
        }
        public async Task<Result<bool,string>> RemovePlayerAsync(string roomId, string sessionId)
        {
            try
            {
                var room = _roomRepository.GetRoomAsync(roomId);
                if (room == null)
                {
                    return "Room not found.";
                }
                var playerSet = await _roomRepository.GetPlayerSetAsync(roomId);
                if(playerSet.Contains(sessionId))
                {
                    await _roomRepository.RemovePlayerFromSetAsync(roomId, sessionId);
                    return true;
                }
                else
                {
                    return "User not in room.";
                }
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error removing player from room. : {error}";
            }
        }
        public async Task<Result<List<string>?,string>> GetPlayerIdsAsync(string roomId)
        {
            try
            {
                var playerSet = await _roomRepository.GetPlayerSetAsync(roomId);
                if (playerSet.Count == 0 || playerSet == null)
                {
                    return "No player id's found! ERROR!!";
                }
                return playerSet;
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error getting player id's. : {error}";
            }
        }
        public async Task SetRoomExpirationAsync(string roomId)
        {
            try
            {
                var filter = Builders<Room>.Filter.Eq(r => r._id, ObjectId.Parse(roomId));
                var room = await _roomRepository.GetRoomAsync(roomId) ?? throw new Exception("Room not found.");
                if(room.Password != null)
                {
                    return;
                }

                DateTime? exp;
                if (room.PlayerCount == 0)
                {
                    exp = DateTime.UtcNow.AddMinutes(30);
                }
                else
                {
                    exp = null;
                }
                var update = Builders<Room>.Update.Set(r => r.ExpirationTime, exp);
                await _roomRepository.UpdateRoomAsync(filter, update);
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
            }
        }

        public async Task<List<string>> GetAllWordPacksAsync()
        {
             return await _roomRepository.GetAllPackNamesAsync();
        }
        public async Task<List<string>> GetWordsByPackNameAsync(string packName)
        {
            return await _roomRepository.GetWordsByPackNameAsync(packName.ToLower());
        }
    }
}
