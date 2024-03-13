using System.ComponentModel.DataAnnotations;

namespace DrawOutApp.Server.Models
{
    public class GameModel
    {
        public string? RoomId { get; set; }
        public TeamModel? RedTeam { get; set; }
        public TeamModel? BlueTeam { get; set; }
        public int TotalRounds { get; set; }
        //public List<RoundModel>? Rounds { get; set; }
        public int CurrentRoundIndex { get; set; }
        //public RoundModel? CurrentRound => Rounds?[CurrentRoundIndex];

        public GameModel()
        {
            //Rounds = new List<RoundModel>(TotalRounds);
        }
    }
}