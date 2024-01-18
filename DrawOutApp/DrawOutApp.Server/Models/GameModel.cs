using System.ComponentModel.DataAnnotations;

namespace DrawOutApp.Server.Models
{
    public class GameModel
    {
        public String? GameSessionId { get; set; }
        public string? RoomId { get; set; }
        public TeamModel? RedTeam { get; set; }
        public TeamModel? BlueTeam { get; set; }
        public int TotalRounds { get; private set; } = 8;
        public List<RoundModel>? Rounds { get; set; }
        public int CurrentRoundIndex { get; set; }
        public RoundModel? CurrentRound => Rounds?[CurrentRoundIndex];

        public GameModel()
        {
            RedTeam = new TeamModel();
            BlueTeam = new TeamModel();
            Rounds = new List<RoundModel>(TotalRounds);
        }
    }
}