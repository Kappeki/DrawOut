
using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Services
{
    public interface IDrawOutDBService
    {
        Task<List<RoomModel>> GetAllRooms();
        RoomModel GetRoomById(int roomId);
        RoomModel CreateRoom(RoomModel room);
        void UpdateRoom(int roomId, RoomModel room);
        void RemoveRoom(int roomId);

    }
}
