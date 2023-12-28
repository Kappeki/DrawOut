using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Mappers
{
    public class GameMapper
    {
        public static GameModel ToModel(Game entity, TeamModel blueTeam, TeamModel redTeam, List<RoundModel> rounds)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var model = new GameModel
            {
                GameSessionId = entity.GameSessionId,
                RoomId = entity.RoomId,
                RedTeam = redTeam,
                BlueTeam = blueTeam,
                Rounds = rounds,
                CurrentRoundIndex = entity.CurrentRoundIndex
            };

            return model;
        }

        public static Game ToEntity(GameModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
           
            var roundIds = model.Rounds.Select(r => r.RoundId).ToList();
            
            var entity = new Game
            {
                GameSessionId = model.GameSessionId,
                RoomId = model.RoomId,
                RedTeamId = model.RedTeam.TeamId,
                BlueTeamId = model.BlueTeam.TeamId,
                TotalRounds = model.TotalRounds,
                RoundIds = roundIds,
                CurrentRoundIndex = model.CurrentRoundIndex
            };

            return entity;
        }
    }
}
