using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;
using DrawOutApp.Server.Repositories;

namespace DrawOutApp.Server.Mappers
{
    public static class TeamMapper
    {
        public static TeamModel ToModel(Team entity, UserModel? teamLeader = null, List<UserModel>? teammates = null)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if(teammates == null)
            {
                teammates = new List<UserModel>();
            }

            return new TeamModel
            {
                GameSessionId = entity.GameSessionId!,
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
                TeammateIds = model.Teammates!.Select(tm => tm.SeshKey).ToList(),
                TeamLeaderId = model.TeamLeader?.SeshKey,
                Score = model.Score,
                GameSessionId = model.GameSessionId
            };
        }
    }
}
