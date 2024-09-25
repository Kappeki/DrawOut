using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Settings;
using MongoDB.Bson.Serialization.Conventions;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Text.Json.Serialization;

namespace DrawOutApp.Server.Repositories
{
    public class GameRepository : IGameRepo
    {
        private readonly IDatabase _database;
        public GameRepository(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }
        public async Task<Game?> GetGameAsync(string gameId)
        {
            var hashEntries = await _database.HashGetAllAsync(gameId);
            if (hashEntries.Length == 0)
            {
                return null;
            }

            var game = new Game
            {
                _id = gameId,
                RoomId = hashEntries.FirstOrDefault(x => x.Name == "RoomId").Value!,
                GameState = Enum.Parse<GameState>(hashEntries.FirstOrDefault(x => x.Name == "GameState").Value!),
                BlueScore = int.Parse(hashEntries.FirstOrDefault(x => x.Name == "BlueScore").Value!),
                RedScore = int.Parse(hashEntries.FirstOrDefault(x => x.Name == "RedScore").Value!),
                TotalRounds = int.Parse(hashEntries.FirstOrDefault(x => x.Name == "TotalRounds").Value!),
                CurrentRound = int.Parse(hashEntries.FirstOrDefault(x => x.Name == "CurrentRound").Value!),
                CurrentPainter = hashEntries.FirstOrDefault(x => x.Name == "CurrentPainter").Value.ToString(),
                SelectedWord = hashEntries.FirstOrDefault(x => x.Name == "SelectedWord").Value,
                TeamLeaders = hashEntries.FirstOrDefault(x => x.Name == "TeamLeaders")
                            .Value
                            .ToString()
                            .Split(',')
                            .Select(s => s.Split(new[] { ':' }, 2)) // Split only at the first colon
                            .ToDictionary(split => split[0], split => split[1]),
                MainTimer = int.Parse(hashEntries.FirstOrDefault(x => x.Name == "MainTimer").Value!),
                StealTimer = int.Parse(hashEntries.FirstOrDefault(x => x.Name == "StealTimer").Value!)
            };
            game.PainterOrder = (await _database.ListRangeAsync($"painter-order:{game.RoomId}")).Select(x => x.ToString()).ToList();

            return game;
        }

        public async Task<string> SaveGameAsync(Game game, TimeSpan? expiry = null)
        {
            if (game == null) throw new ArgumentNullException(nameof(game));

            if(game.RoomId == null) throw new ArgumentException("RoomId must be set.", nameof(game.RoomId));

            game._id = $"game:{game.RoomId}";

            await _database.KeyDeleteAsync($"painter-order:{game.RoomId}");

            var teamLeadersSerialized = game.TeamLeaders != null ? string
                .Join(",", game.TeamLeaders.Select(kv => $"{kv.Key}:{kv.Value}")) : string.Empty;


            await _database.HashSetAsync(game._id,
            [
                new HashEntry("RoomId", game.RoomId ?? string.Empty),
                new HashEntry("GameState", game.GameState.ToString()),
                new HashEntry("BlueScore", game.BlueScore.ToString()),
                new HashEntry("RedScore", game.RedScore.ToString()),
                new HashEntry("TotalRounds", game.TotalRounds.ToString()),
                new HashEntry("PainterOrder", $"painter-order:{game.RoomId}"),
                new HashEntry("CurrentRound", game.CurrentRound.ToString()),
                new HashEntry("CurrentPainter", game.CurrentPainter ?? string.Empty),
                new HashEntry("SelectedWord", game.SelectedWord ?? string.Empty),
                new HashEntry("TeamLeaders", teamLeadersSerialized),
                new HashEntry("MainTimer", game.MainTimer.ToString()),
                new HashEntry("StealTimer", game.StealTimer.ToString())
            ]);

            foreach(var painter in game.PainterOrder!)
            {
                await _database.ListRightPushAsync($"painter-order:{game.RoomId}", painter);
            }

            if (expiry.HasValue)
            {
                await _database.KeyExpireAsync($"painter-order:{game.RoomId}", expiry);
                await _database.KeyExpireAsync(game._id, expiry);
            }

            return game._id;
        }
        public async Task<Game?> UpdateGameRoundAsync(GameRoundModel gameRound, TimeSpan? expiry = null)
        {
            if (gameRound == null) throw new ArgumentNullException(nameof(gameRound));

            await _database.HashSetAsync(gameRound._gameId,
            [
                new HashEntry("GameState", gameRound.GameState!.ToString()),
                new HashEntry("BlueScore", gameRound.BlueScore.ToString()),
                new HashEntry("RedScore", gameRound.RedScore.ToString()),
                new HashEntry("CurrentRound", gameRound.CurrentRound.ToString()),
                new HashEntry("CurrentPainter", gameRound.CurrentPainter ?? string.Empty),
                new HashEntry("SelectedWord", gameRound.SelectedWord ?? string.Empty)
            ]);

            if (expiry.HasValue)
            {
                await _database.KeyExpireAsync(gameRound._gameId, expiry);
            }

            return await GetGameAsync(gameRound._gameId);
        }

        public async Task<int> IncrementScoreAsync(string gameId, string teamName, int incrementValue)
        {
            if (incrementValue <= 0) throw new ArgumentException("Increment value must be positive.", nameof(incrementValue));

            var field = $"{teamName}Score";
            await _database.HashIncrementAsync(gameId, field, incrementValue);
            return int.Parse((await _database.HashGetAsync(gameId, field))!);
        }

        public async Task DeleteGameAsync(string gameId)
        {
            if (!_database.KeyExists(gameId)) { throw new ArgumentException("GameSessionId does not exist"); }
            await _database.KeyDeleteAsync(gameId);
        }
    }
}
