using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Repositories.Contracts;
using DrawOutApp.Server.Settings;
using StackExchange.Redis;
using Newtonsoft.Json;
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

        public async Task AddTeamAsync(Team team)
        {
            var teamJson = JsonConvert.SerializeObject(team);
            await _database.StringSetAsync(team.TeamId, teamJson);
        }

        public async Task DeleteTeamAsync(string teamId)
        {
            await _database.KeyDeleteAsync(teamId);
        }

        public async Task<Team?> GetTeamAsync(string teamId)
        {
            var teamJson = await _database.StringGetAsync(teamId);
            if (!teamJson.IsNullOrEmpty)
            {
                return JsonConvert.DeserializeObject<Team>(teamJson);
            }
            return null;
        }

        public async Task UpdateTeamAsync(string teamId, Team team)
        {
            var teamJson = JsonConvert.SerializeObject(team);
            await _database.StringSetAsync(teamId, teamJson);
        }
    }
}
