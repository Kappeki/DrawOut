using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Settings;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace DrawOutApp.Server.Repositories
{
    public class ChatMessageRepository : IChatMessageRepo
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private const string ChatMessageKeyPrefix = "chatMessage:";

        public ChatMessageRepository(IRedisSettings settings)
        {
            _redis = ConnectionMultiplexer.Connect(settings.ConnectionString);
            _database = _redis.GetDatabase();
        }

        public async Task AddChatMessageAsync(ChatMessage chatMessage, string roomId)
        {
            string key = $"{ChatMessageKeyPrefix}{roomId}";
            string serializedMessage = JsonConvert.SerializeObject(chatMessage);

            // Add the serialized chat message to the list associated with the room
            await _database.ListRightPushAsync(key, serializedMessage);
        }

        public async Task<List<ChatMessage>> GetChatMessagesAsync(string roomId, int count = 50)
        {
            string key = $"{ChatMessageKeyPrefix}{roomId}";
            var messages = await _database.ListRangeAsync(key, -count, -1);

            return messages.Select(m => JsonConvert.DeserializeObject<ChatMessage>(m)).ToList();
        }
    }
}
