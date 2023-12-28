using DrawOutApp.Server.Entities;

namespace DrawOutApp.Server.Repositories.Contracts
{
    public interface IChatMessageRepo
    {
        public Task AddMessageToRoomChatAsync(ChatMessage chatMessage, string roomId);
        public Task AddMessageToRoundChatAsync(ChatMessage chatMessage, string roundId);
        public Task<IEnumerable<ChatMessage?>> GetRoomChatAsync(string roomId);
        public Task<IEnumerable<ChatMessage?>> GetRoundChatAsync(string roundId);
    }
}
