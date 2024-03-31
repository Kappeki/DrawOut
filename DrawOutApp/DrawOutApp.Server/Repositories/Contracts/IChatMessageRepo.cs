using DrawOutApp.Server.Entities;

namespace DrawOutApp.Server.Repositories.Contracts
{
    public interface IChatMessageRepo
    {
        public Task AddToChatAsync(string roomId, ChatMessage msg);
        public Task<IEnumerable<ChatMessage?>> GetRoomChatAsync(string roomId);
        public Task ClearChatAsync(string roomId);
    }
}
