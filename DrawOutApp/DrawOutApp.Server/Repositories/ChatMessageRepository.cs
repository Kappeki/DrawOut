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
        private const string RoomChatKeyPrefix = "roomChat:";
        private const string RoundChatKeyPrefix = "roundChat:";

        public ChatMessageRepository(IRedisSettings settings)
        {
            _redis = ConnectionMultiplexer.Connect(settings.ConnectionString);
            _database = _redis.GetDatabase();
        }
        public async Task AddMessageToRoomChatAsync(ChatMessage message, string roomId)
        {
            string key = $"{RoomChatKeyPrefix}{roomId}";
            string serializedMessage = JsonConvert.SerializeObject(message);
            await _database.ListRightPushAsync(key, serializedMessage);
        }

        public async Task<IEnumerable<ChatMessage?>> GetRoomChatAsync(string roomId)
        {
            string key = $"{RoomChatKeyPrefix}{roomId}";
            var serializedMessages = await _database.ListRangeAsync(key);
            return serializedMessages.Select(msg => JsonConvert.DeserializeObject<ChatMessage>(msg!));
        }

        public async Task AddMessageToRoundChatAsync(ChatMessage message, string roundId)
        {
            string key = $"{RoundChatKeyPrefix}{roundId}";
            string serializedMessage = JsonConvert.SerializeObject(message);
            await _database.ListRightPushAsync(key, serializedMessage);
        }

        public async Task<IEnumerable<ChatMessage?>> GetRoundChatAsync(string roundId)
        {
            string key = $"{RoundChatKeyPrefix}{roundId}";
            var serializedMessages = await _database.ListRangeAsync(key);
            return serializedMessages.Select(msg => JsonConvert.DeserializeObject<ChatMessage>(msg!));
        }
    }
}
