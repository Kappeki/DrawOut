using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Settings;
using StackExchange.Redis;
using Newtonsoft.Json;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Reflection;
namespace DrawOutApp.Server.Repositories
{
    public class TeamRepository : ITeamRepo
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        public TeamRepository(IRedisSettings settings)
        {
            _redis = ConnectionMultiplexer.Connect(settings.ConnectionString);
            _database = _redis.GetDatabase();
        }

        public ITransaction BeginTransaction()
        {
            return _database.CreateTransaction();
        }

        public async Task AddOrUpdateTeamAsync(Team team, TimeSpan? expiry = null)
        {
            if(team == null) throw new ArgumentNullException(nameof(team));

            var teamListKey = $"teamlist:{team._cacheKey}";
            
            if(team.TeammateIds != null && team.TeammateIds.Any())
            {
                await _database.ListRightPushAsync(teamListKey, 
                    team.TeammateIds.Select(id => (RedisValue)id).ToArray());
            }

            await _database.HashSetAsync(team._cacheKey, [
                new("TeammateListKey", teamListKey),
                new("TeamLeaderId", team.TeamLeaderId ?? string.Empty),
                new("GameSessionId", team.GameSessionId ?? string.Empty),
                new("Score", team.Score)
            ]); 

            if(expiry.HasValue)
            {
                await _database.KeyExpireAsync(team._cacheKey, expiry);
            }
        }

        public async Task AddOrUpdateTeamAsync(Team team, ITransaction tran, TimeSpan? expiry = null)
        {
            if(team == null) throw new ArgumentNullException(nameof(team));

            var teamListKey = $"teamlist:{team._cacheKey}";
            
            if(team.TeammateIds != null && team.TeammateIds.Any())
            {
                await tran.ListRightPushAsync(teamListKey, 
                    team.TeammateIds.Select(id => (RedisValue)id).ToArray());
            }

            await tran.HashSetAsync(team._cacheKey, [
                new("TeammateListKey", teamListKey),
                new("TeamLeaderId", team.TeamLeaderId ?? string.Empty),
                new("GameSessionId", team.GameSessionId ?? string.Empty),
                new("Score", team.Score)
            ]); 

            if(expiry.HasValue)
            {
                await tran.KeyExpireAsync(team._cacheKey, expiry);
            }
        }

        public async Task DeleteTeamAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }

        public async Task<Team?> GetTeamAsync(string key)
        {
            var hashEntries = await _database.HashGetAllAsync(key);
            if (hashEntries.Length == 0)
            {
                return null;
            }

            Team team = new Team();
            foreach (var entry in hashEntries)
            {
                string propName = entry.Name.ToString();
                PropertyInfo? propInfo = typeof(Team).GetProperty(propName);

                if (propInfo != null && propInfo.CanWrite)
                {
                    if(propName == "TeammateListKey")
                    {
                        var teammateIds = await _database.ListRangeAsync(entry.Value.ToString());
                        team.TeammateIds = teammateIds.Select(id => id.ToString()).ToList();
                    }
                    else
                    {
                        object? propValue = JsonConvert.DeserializeObject(entry.Value!, propInfo.PropertyType);
                        propInfo.SetValue(team, propValue);
                    }
                }
            }

            return team;
        }

        //update score
        //update team leader
        //remove teammate
        //add teammate

        public async Task AddTeammateAsync(string cacheKey, string teammateId)
        {
            var teamListKey = $"teamlist:{cacheKey}";
            await _database.ListRightPushAsync(teamListKey, teammateId);
        }
        public async Task RemoveTeammateAsync(string cacheKey, string teammateId)
        {
            var teamListKey = $"teamlist:{cacheKey}";
            await _database.ListRemoveAsync(teamListKey, teammateId, 0);
        }
        public async Task UpdateScoreAsync(string cacheKey, int score)
        {
            await _database.HashIncrementAsync(cacheKey, "Score", score);
        }
        public async Task UpdateTeamLeaderAsync(string cacheKey, string teamLeaderId)
        {
            await _database.HashSetAsync(cacheKey, "TeamLeaderId", teamLeaderId);
        }
    }
}
