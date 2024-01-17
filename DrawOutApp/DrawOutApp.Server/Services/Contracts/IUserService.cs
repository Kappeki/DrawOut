using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Services.Contracts
{
    public interface IUserService
    {
        //poziva se kad se klikne play 
        Task<Result<UserModel,string>> CreateUserAsync(UserModel userModel);
        //se poziva kad se vrati ponovo na sajt
        Task<Result<UserModel?,string>> GetUserAsync(string sessionId);
        //poziva se kad se promeni tim, nickname, iconUrl, gameSessionId
        Task<Result<bool, string>> UpdateUserAsync(string sessionId, UserModel userModel);

        Task AddRole(string sessionId, Role role);
        Task RemoveRole(string sessionId, Role role);
        Task DeleteUserAsync(string sessionId);
    }
}
