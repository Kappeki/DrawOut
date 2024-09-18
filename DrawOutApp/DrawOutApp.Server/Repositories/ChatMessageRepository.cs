using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Settings;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace DrawOutApp.Server.Repositories
{
    public class ChatMessageRepository : IChatMessageRepo
    {
        private readonly IDatabase _database;
        public ChatMessageRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task AddToChatAsync(string roomId, ChatMessage msg)
        {
            var key = $"chat:{roomId}";
            
            await _database.ListRightPushAsync(key, JsonConvert.SerializeObject(msg));
            await _database.ListTrimAsync(key, 0, 100); //cuva poslednje 100 poruke
            await _database.KeyExpireAsync(key, TimeSpan.FromDays(30));
        }

        public async Task<IEnumerable<ChatMessage?>> GetRoomChatAsync(string roomId)
        {
            var key = $"chat:{roomId}";
            var chatMessages = await _database.ListRangeAsync(key);
            return chatMessages.Select(msg => JsonConvert.DeserializeObject<ChatMessage>(msg!));
        }

        //called when a game starts, when round ends...
        public async Task ClearChatAsync(string roomId)
        {
            var key = $"chat:{roomId}";
            await _database.KeyDeleteAsync(key);
        }
    }
}
