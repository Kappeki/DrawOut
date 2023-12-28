using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Repositories;

namespace DrawOutApp.Server.Mappers
{
    public static class TeamMapper
    {
        public static TeamModel ToModel(Team entity, UserModel teamLeader, List<UserModel> teammates)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            return new TeamModel
            {
                TeamId = entity.TeamId,
                GameSessionId = entity.GameSessionId,
                Teammates = teammates, 
                TeamLeader = teamLeader,
                Score = entity.Score
            };
        }

        public static Team ToEntity(TeamModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            return new Team
            {
                TeamId = model.TeamId,
                TeammateIds = model.Teammates.Select(tm => tm.SessionId).ToList(),
                TeamLeaderId = model.TeamLeader?.SessionId,
                Score = model.Score,
                GameSessionId = model.GameSessionId
            };
        }
    }
}
