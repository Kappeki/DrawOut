using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Services.Contracts
{
    public interface IRoomService
    {
        // ovo se poziva kad se klikne create room
        Task<RoomModel> CreateRoomAsync(RoomModel roomModel, string creatingUserId);
        // ovo se poziva kad se klikne join room
        Task<RoomModel?> GetRoomAsync(string roomId);
        //ovo treba za listu svih aktivnih soba
        Task<IEnumerable<RoomModel>> GetAllRoomsAsync();
        Task UpdateRoomAsync(RoomModel roomModel);
        Task DeleteRoomAsync(string roomId);
    }
}
