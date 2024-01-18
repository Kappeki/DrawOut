using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Services.Contracts
{
    public interface IRoomService
    {
        // ovo se poziva kad se klikne create room
        Task<Result<RoomModel,string>> CreateRoomAsync(RoomModel roomModel, string creatingUserId);
        // ovo se poziva kad se klikne join room
        Task<Result<Username,string>> AddPlayer(string roomId, string sessionId);
        Task<Result<Username,string>> RemovePlayer(string roomId, string sessionId);
        Task<Result<RoomModel?,string>> GetRoomAsync(string roomId);
        //ovo treba za listu svih aktivnih soba
        Task<Result<List<RoomModel>,string>> GetAllRoomsAsync();
        Task<Result<bool,string>> UpdateRoomAsync(RoomModel roomModel);
        Task DeleteRoomAsync(string roomId);
    }
}
