using DrawOutApp.Server.DataLayer;
using DrawOutApp.Server.Models;
using MongoDB.Driver;

namespace DrawOutApp.Server.Services
{
    public class DrawOutDBService : IDrawOutDBService
    {

        private readonly IMongoCollection<Room> _roomsCollection;

        public DrawOutDBService(IMongoDBSettings settings, IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _roomsCollection = database.GetCollection<Room>(settings.RoomsCollectionName);
        }
        public Room CreateRoom(Room room)
        {
            throw new NotImplementedException();
        }

        public Task<List<Room>> GetAllRooms()
        {
            throw new NotImplementedException();
        }

        public Room GetRoomById(int roomId)
        {
            throw new NotImplementedException();
        }

        public void RemoveRoom(int roomId)
        {
            throw new NotImplementedException();
        }

        public void UpdateRoom(int roomId, Room room)
        {
            throw new NotImplementedException();
        }
    }
}
