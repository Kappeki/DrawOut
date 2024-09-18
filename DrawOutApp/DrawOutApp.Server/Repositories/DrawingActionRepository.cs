using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Settings;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace DrawOutApp.Server.Repositories
{
    public class DrawingActionRepository : IDrawingActionRepo
    {
        private readonly IDatabase _database;
        public DrawingActionRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task AddDrawingActionAsync(DrawingAction action)
        {
            string key = $"draw_actions:{action.GameId.Split(':')[1]}:{action.PainterName}";
            string actionJson = JsonConvert.SerializeObject(action);
            double score = action.Timestamp;

            await _database.SortedSetAddAsync(key, actionJson, score);
        }

        public async Task ClearDrawingActionsAsync(string roomId, string painterName)
        {
            string key = $"draw_actions:{roomId}:{painterName}";
            await _database.KeyDeleteAsync(key);
        }

        public async Task<List<DrawingAction>> GetDrawingActionsSinceAsync(string roomId, string painterName, long sinceTimestamp)
        {
            string key = $"draw_actions:{roomId}:{painterName}";
            var actionJsonList = await _database.SortedSetRangeByScoreAsync(key, sinceTimestamp, double.MaxValue);

            return actionJsonList
                .Select(actionJson => JsonConvert.DeserializeObject<DrawingAction>(actionJson!))
                .ToList()!;
        }

        public async Task<List<DrawingAction>> GetLastNActionsAsync(string roomId, string painterName, int n)
        {
            string key = $"draw_actions:{roomId}:{painterName}";
            var actionJsonList = await _database.SortedSetRangeByRankAsync(key, -n, -1);

            return actionJsonList
                .Select(actionJson => JsonConvert.DeserializeObject<DrawingAction>(actionJson!))
                .ToList()!;
        }
        public async Task UndoStrokeAsync(string roomId, string painterName, string strokeId)
        {
            string key = $"draw_actions:{roomId}:{painterName}";

            var actionJsonList = await _database.SortedSetRangeByValueAsync(key, $"*{strokeId}*");

            foreach (var actionJson in actionJsonList)
            {
                await _database.SortedSetRemoveAsync(key, actionJson);
            }
        }
    }
}
