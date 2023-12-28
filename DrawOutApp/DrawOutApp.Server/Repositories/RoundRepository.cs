using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Settings;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace DrawOutApp.Server.Repositories
{
    public class RoundRepository : IRoundRepo
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        public RoundRepository(IRedisSettings settings)
        {
            _redis = ConnectionMultiplexer.Connect(settings.ConnectionString);
            _database = _redis.GetDatabase();
        }

        //nema potrebe jer se runda kreira u GameRepository
        /*public async Task AddRoundAsync(Round round)
        {
            if (round == null) throw new ArgumentNullException(nameof(round));
            var roundJson = JsonConvert.SerializeObject(round);
            await _database.StringSetAsync(round.RoundId, roundJson);
        }*/

        public async Task DeleteRoundAsync(string roundId)
        {
            if (!_database.KeyExists(roundId)) throw new ArgumentException($"Round does not exist with ID {roundId}");
            await _database.KeyDeleteAsync(roundId);
        }

        public async Task<Round?> GetRoundAsync(string roundId)
        {
            var roundJson = await _database.StringGetAsync(roundId);
            if (!roundJson.IsNullOrEmpty)
            {
                return JsonConvert.DeserializeObject<Round>(roundJson);
            }
            return null;
        }
        public async Task UpdateRoundAsync(string roundId, Round round)
        {
            if (roundId != round.RoundId) throw new ArgumentException("RoundId does not match");
            var roundJson = JsonConvert.SerializeObject(round);
            await _database.StringSetAsync(roundId, roundJson);
        }


        //nema potrebe za drawing action repo jer se sve radi u round repo
        public async Task<List<DrawingAction>> GetDrawingActionsForRoundAsync(string roundId)
        {
            var roundJson = await _database.StringGetAsync(roundId);
            if (!roundJson.IsNullOrEmpty)
            {
                var round = JsonConvert.DeserializeObject<Round>(roundJson);
                return round.DrawingActions;
            }
            return new List<DrawingAction>();
        }

        public async Task AddDrawingActionToRoundAsync(string roundId, DrawingAction action)
        {
            var roundJson = await _database.StringGetAsync(roundId);
            if (!roundJson.IsNullOrEmpty)
            {
                var round = JsonConvert.DeserializeObject<Round>(roundJson);
                round.DrawingActions.Add(action);
                var updatedRoundJson = JsonConvert.SerializeObject(round);
                await _database.StringSetAsync($"round:{roundId}", updatedRoundJson);

                // Publish the action to the round's Pub/Sub channel for real-time updates
                await _database.PublishAsync($"roundChannel:{roundId}", JsonConvert.SerializeObject(action));
            }
            else
            {
                throw new KeyNotFoundException($"Round with ID {roundId} not found.");
            }
        }
    }
}
