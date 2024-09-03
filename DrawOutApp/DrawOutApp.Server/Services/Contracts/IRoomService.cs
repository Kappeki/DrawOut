using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;
using System.Linq.Expressions;

namespace DrawOutApp.Server.Services.Contracts
{
    public interface IRoomService
    {
        // ovo se poziva kad se klikne create room
        Task<Result<RoomModel,string>> CreateRoomAsync(string creatingUserId, string roomName, string? password = null);
        // ovo se poziva kad se klikne join room
        //Task<Result<RoomModel, string>> AddPlayerAsync(RoomModel roomModel, string sessionId, string? password = null);
        Task<Result<bool, string>> AddPlayerAsync(string roomId, string sessionId, string? password = null);
        Task<Result<bool, string>> RemoveUserAsync(string roomId, string sessionId);
        Task<Result<RoomModel?,string>> GetRoomByIdAsync(string roomId);
        Task<Result<RoomModel?, string>> GetRoomByUrlAsync(string roomUrl);
        //ovo treba za listu svih aktivnih soba
        Task<Result<List<RoomListItem>?,string>> GetAllRoomsAsync(string sessionId, bool? isAscending = null, bool? isProtected = null);
        Task<Result<List<RoomListItem>?, string>> GetMyRoomsAsync(string sessionId);
        Task<Result<bool,string>> UpdateRoomAsync(RoomModel roomModel);
        Task UpdateRoomStateAsync(string roomId, RoomState roomState);
        Task DeleteRoomAsync(string roomId);
        Task<string?> GetIdFromURL(string roomURL);
        Task<Result<List<string>?, string>> GetPlayerIdsAsync(string roomId);
        Task<List<string>> GetAllWordPacksAsync();
        Task<List<string>> GetWordsByPackNameAsync(string packName);
    }
}
