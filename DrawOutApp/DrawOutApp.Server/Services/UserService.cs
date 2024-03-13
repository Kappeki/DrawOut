using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Mappers;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Services.Contracts;

namespace DrawOutApp.Server.Services
{
    public class UserService : IUserService
    {

        private readonly IUserRepo _userRepo;

        public UserService(IUserRepo userRepository)
        {
            _userRepo = userRepository;
        }

        public async Task<Result<UserModel?, string>> GetUserAsync(string sessionId)
        {
            UserModel userModel = default!;
            try
            {
                var userHash = await _userRepo.GetUserAsync(sessionId);
                if (userHash == null) return "User not found with Session ID: " + sessionId;

                userModel = UserMapper.ToModel(userHash);
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return "Failed to get user with error : " + error;
            }
            return userModel;
        }

        //ove dve funkcije bi trebalo da se trigeruju ili kad se promeni icon/username na front page
        //ili kad se klikne na play dugme - ovo je verovatno jednostavnije
        public async Task<Result<UserModel, string>> CreateUserSessionAsync(UserModel userModel)
        {
            try
            {
                User newUser = new User()
                {
                    Nickname = userModel.Nickname,
                    Icon = userModel.Icon
                };

                if (string.IsNullOrEmpty(newUser.Nickname))
                {
                    newUser.Nickname = "Guest";
                    //newUser.Icon = "user.png";
                }
                await _userRepo.AddOrUpdateUserAsync(newUser, TimeSpan.FromDays(7));

                return UserMapper.ToModel(newUser);
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error while creating user : {error}";
            }
        }

        public async Task<Result<bool, string>> UpdateUserPrefsAsync(string sessionId, UserModel userModel)
        {
            try
            {
                var user = await _userRepo.GetUserAsync(sessionId);
                if (user == null) return "User not found with Session ID: " + sessionId;

                if (userModel.Nickname != user.Nickname || userModel.Icon != user.Icon)
                {
                    user.Nickname = userModel.Nickname;
                    user.Icon = userModel.Icon;
                }

                user = UserMapper.ToEntity(userModel);
                await _userRepo.AddOrUpdateUserAsync(user, TimeSpan.FromDays(7));
                return true;

            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Failed to update user with error : {error}";
            }
        }

        public async Task AddRole(string roomId, string sessionId, Role role)
        {
            var user = await _userRepo.GetUserAsync(sessionId);
            if (user != null)
            {
                user.Roles!.Add(role);
                await _userRepo.UpdateUserInRoomAsync(roomId, user, new Dictionary<string, object>
                {
                    {"Roles", user.Roles }
                });
            }
            else
            {
                throw new KeyNotFoundException($"User not found with Session ID: {sessionId}");
            }
        }

        public async Task RemoveRole(string roomId, string sessionId, Role role)
        {
            var user = await _userRepo.GetUserAsync(sessionId);
            if (user != null)
            {
                user.Roles!.Remove(role);
                await _userRepo.UpdateUserInRoomAsync(roomId, user, new Dictionary<string, object>
                {
                    {"Roles", user.Roles}
                });
            }
            else
            {
                throw new KeyNotFoundException($"User not found with Session ID: {sessionId}");
            }
        }

        //za testiranje
        public async Task DeleteUserAsync(string sessionId)
        {
            await _userRepo.DeleteUserAsync(sessionId);
        }
    }
}
