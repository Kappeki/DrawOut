using DrawOutApp.Server.Entities;

namespace DrawOutApp.Server.Repositories.Contracts
{
    public interface IChatMessageRepo
    {
        public Task AddChatMessageAsync(ChatMessage chatMessage, string roomId);
        public Task<IEnumerable<ChatMessage>> GetChatMessagesAsync(string roomId, int count = 50);
    }
}
