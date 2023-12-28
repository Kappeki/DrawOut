using DrawOutApp.Server.Mappers;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Repositories;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Services.Contracts;

namespace DrawOutApp.Server.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepo _roomRepository;
        private readonly IUserService _userService;
        public RoomService(IRoomRepo roomRepository, IUserService userService, ITeamService teamService)
        {
            _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository)); 
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<RoomModel> CreateRoomAsync(RoomModel roomModel, string creatingUserId)
        { 

            UserModel adminUser = await _userService.GetUserAsync(creatingUserId);
            if (adminUser == null)
            {
                throw new ArgumentException("Creating user not found.");
            }

            roomModel.RoomAdmin = adminUser;
            if (!adminUser.Roles.Contains(Role.RoomAdmin))
            {
                await _userService.AddRole(creatingUserId, Role.RoomAdmin);
            }

            var roomEntity = RoomMapper.ToEntity(roomModel);
            await _roomRepository.CreateRoomAsync(roomEntity);
            return RoomMapper.ToModel(roomEntity);
        }

        public async Task<RoomModel?> GetRoomAsync(string roomId)
        {
            var roomEntity = await _roomRepository.GetRoomAsync(roomId);
            return roomEntity != null ? RoomMapper.ToModel(roomEntity) : null;
        }

        public async Task<IEnumerable<RoomModel>> GetAllRoomsAsync()
        {
            var roomEntities = await _roomRepository.GetAllRoomsAsync();
            return roomEntities.Select(RoomMapper.ToModel);
        }

        public async Task UpdateRoomAsync(RoomModel roomModel)
        {
            var roomEntity = RoomMapper.ToEntity(roomModel);
            await _roomRepository.UpdateRoomAsync(roomEntity);
        }

        public async Task DeleteRoomAsync(string roomId)
        {
            await _roomRepository.DeleteRoomAsync(roomId);
        }
    }
}
