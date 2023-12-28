using DrawOutApp.Server.Entities;

namespace DrawOutApp.Server.Repositories.Contracts
{
    public interface IRoomRepo
    {
        Task<Room> CreateRoomAsync(Room room);
        Task<Room?> GetRoomAsync(string roomId);
        Task<IEnumerable<Room>> GetAllRoomsAsync();
        Task UpdateRoomAsync(Room room);
        Task DeleteRoomAsync(string roomId);
    }
}
