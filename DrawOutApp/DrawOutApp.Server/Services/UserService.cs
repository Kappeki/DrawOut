using AutoMapper;
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
        private readonly IMapper _mapper;

        public UserService(IUserRepo userRepository, IMapper mapper)
        {
            _userRepo = userRepository;
            _mapper = mapper;
        }

        public async Task<Result<UserModel?, string>> GetUserAsync(string sessionId)
        {
            UserModel userModel = default!;
            try
            {
                var userHash = await _userRepo.GetUserAsync(sessionId);
                if (userHash == null) return "User not found with Session ID: " + sessionId;

                userModel = _mapper.Map<UserModel>(userHash);
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return "Failed to get user with error : " + error;
            }
            return userModel;
        }
        public async Task<Result<UserModel?, string>> GetUserSessionAsync(HttpRequest request)
        {
            UserModel userModel = default!;
            try
            {
                var seshKey = request.Cookies["UserSessionId"];
                if (string.IsNullOrEmpty(seshKey))
                {
                    return "Session not found";
                }
                var userHash = await _userRepo.GetUserAsync(seshKey);
                if (userHash == null) return "User not found with Session ID: " + seshKey;

                userModel = _mapper.Map<UserModel>(userHash);
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return "Failed to get user with error : " + error;
            }
            return userModel;
        }
        public async Task<Result<UserModel, string>> CreateUserSessionAsync(UserPreferences userModel)
        {
            try
            {
                User newUser = new User()
                {
                    Nickname = userModel.Nickname,
                    Icon = userModel.Icon
                };

                await _userRepo.AddOrUpdateUserAsync(newUser, TimeSpan.FromDays(7));

                return _mapper.Map<UserModel>(newUser);
            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Error while creating user : {error}";
            }
        }
        public async Task<Result<bool, string>> UpdateUserPrefsAsync(string sessionId, UserPreferences userModel)
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

                await _userRepo.AddOrUpdateUserAsync(user, TimeSpan.FromDays(7));
                return true;

            }
            catch (Exception ex)
            {
                string error = ErrorHandler.HandleError(ex);
                return $"Failed to update user with error : {error}";
            }
        }
        public async Task AddRolesAsync(string sessionId, IEnumerable<Role> roles)
        {
            var user = await _userRepo.GetUserAsync(sessionId);
            if (user != null)
            {
                user.Roles ??= new HashSet<Role>();
                foreach (var role in roles)
                {
                    user.Roles.Add(role);
                }
                await _userRepo.AddOrUpdateUserAsync(user);
            }
            else
            {
                throw new KeyNotFoundException($"User not found with Session ID: {sessionId}");
            }
        }
        public async Task RemoveRolesAsync(string sessionId, IEnumerable<Role> roles)
        {
            var user = await _userRepo.GetUserAsync(sessionId);
            if (user != null && user.Roles != null)
            {
                foreach (var role in roles)
                {
                    user.Roles.Remove(role);
                }
                await _userRepo.AddOrUpdateUserAsync(user);
            }
            else
            {
                throw new KeyNotFoundException($"User not found with Session ID: {sessionId}");
            }
        }

        //koriste se za signalr komunikaciju
        public async Task SetConnectionIdAsync(string sessionId, string connectionId, TimeSpan? expiry = null)
        {
            var user = await _userRepo.GetUserAsync(sessionId);
            if (user != null)
            {
                await _userRepo.AddToHashSet(sessionId, _ => "ConnectionId", connectionId, expiry);
            }
            else
            {
                throw new KeyNotFoundException($"User not found with Session ID: {sessionId}");
            }
        }
        public async Task<string?> GetConnectionIdAsync(string sessionKey)
        {
            return await _userRepo.GetFromHashSet<string>(sessionKey, "ConnectionId");
        }


        //za testiranje
        public async Task DeleteUserAsync(string sessionId)
        {
            await _userRepo.DeleteUserAsync(sessionId);
        }

        //dodato
        public async Task<string> GetRandomNicknameAsync()
        {
            var nicknames = await _userRepo.GetAllNicknamesAsync();
            if (nicknames == null || !nicknames.Any())
            {
                return string.Empty;
            }
            var random = new Random();
            return nicknames[random.Next(nicknames.Count)];
        }

        public async Task<string> GetRandomIconAsync()
        {
            var icons = await _userRepo.GetAllIconsAsync();
            if (icons == null || !icons.Any())
            {
                return string.Empty;
            }
            var random = new Random();
            var randomIconData = icons[random.Next(icons.Count)];

            return $"data:image/jpeg;base64,{randomIconData}";
        }
    }
}
