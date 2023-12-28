using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Services.Contracts
{
    public interface IUserService
    {
        //poziva se kad se klikne play 
        Task<UserModel> CreateUserAsync(UserModel userModel);
        //se poziva kad se vrati ponovo na sajt
        Task<UserModel?> GetUserAsync(string sessionId);
        //poziva se kad se promeni tim, nickname, iconUrl, gameSessionId
        Task UpdateUserAsync(string sessionId, UserModel userModel);

        Task AddRole(string sessionId, Role role);
        Task RemoveRole(string sessionId, Role role);
        Task DeleteUserAsync(string sessionId);
    }
}
