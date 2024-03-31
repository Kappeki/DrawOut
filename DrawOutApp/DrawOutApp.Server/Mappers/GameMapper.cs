using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Mappers
{
    public class GameMapper
    {
        public static GameModel ToModel(Game entity, TeamModel blueTeam, TeamModel redTeam)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var model = new GameModel
            {
                CacheKey = entity._cacheKey,
                RoomId = entity.RoomId,
                RedTeam = redTeam,
                BlueTeam = blueTeam,
                TotalRounds = entity.TotalRounds,
                CurrentRoundIndex = entity.CurrentRoundIndex
            };

            return model;
        }

        public static Game ToEntity(GameModel model, string redTeamId, string blueTeamId)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var entity = new Game
            {
                RoomId = model.RoomId,
                RedTeamId = redTeamId,
                BlueTeamId = blueTeamId,
                TotalRounds = model.TotalRounds,
                CurrentRoundIndex = model.CurrentRoundIndex
            };

            return entity;
        }
    }
}
