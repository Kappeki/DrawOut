
using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Services
{
    public interface IDrawOutDBService
    {
        Task<List<Room>> GetAllRooms();
        Room GetRoomById(int roomId);
        Room CreateRoom(Room room);
        void UpdateRoom(int roomId, Room room);
        void RemoveRoom(int roomId);

    }
}
