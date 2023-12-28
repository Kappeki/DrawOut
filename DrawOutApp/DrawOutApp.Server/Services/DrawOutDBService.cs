using DrawOutApp.Server.Settings;
using DrawOutApp.Server.Models;
using MongoDB.Driver;

namespace DrawOutApp.Server.Services
{
    public class DrawOutDBService : IDrawOutDBService
    {

        private readonly IMongoCollection<RoomModel> _roomsCollection;

        public DrawOutDBService(IMongoDBSettings settings, IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _roomsCollection = database.GetCollection<RoomModel>(settings.RoomsCollectionName);
        }
        public RoomModel CreateRoom(RoomModel room)
        {
            throw new NotImplementedException();
        }

        public Task<List<RoomModel>> GetAllRooms()
        {
            throw new NotImplementedException();
        }

        public RoomModel GetRoomById(int roomId)
        {
            throw new NotImplementedException();
        }

        public void RemoveRoom(int roomId)
        {
            throw new NotImplementedException();
        }

        public void UpdateRoom(int roomId, RoomModel room)
        {
            throw new NotImplementedException();
        }
    }
}
