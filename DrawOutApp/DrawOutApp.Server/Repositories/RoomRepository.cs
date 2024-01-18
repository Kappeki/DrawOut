using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Settings;
using MongoDB.Driver;

namespace DrawOutApp.Server.Repositories
{
    public class RoomRepository : IRoomRepo
    {
        private readonly IMongoCollection<Room> _roomsCollection;

        public RoomRepository(IMongoDBSettings settings, IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _roomsCollection = database.GetCollection<Room>(settings.RoomsCollectionName);
        }

        public async Task<Room> CreateRoomAsync(Room room)
        {
            await _roomsCollection.InsertOneAsync(room);
            return room;
        }

        public async Task<Room?> GetRoomAsync(string roomId)
        {
            return await _roomsCollection.Find(r => r.RoomId == roomId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Room>> GetAllRoomsAsync()
        {
            return await _roomsCollection.Find(_ => true).ToListAsync();
        }

        public async Task<bool> UpdateRoomAsync(Room room)
        {
            await _roomsCollection.ReplaceOneAsync(r => r.RoomId == room.RoomId, room);
            return true;
        }

        public async Task DeleteRoomAsync(string roomId)
        {
            await _roomsCollection.DeleteOneAsync(r => r.RoomId == roomId);
        }
    }
}
