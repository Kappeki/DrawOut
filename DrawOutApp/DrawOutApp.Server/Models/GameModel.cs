using System.ComponentModel.DataAnnotations;

namespace DrawOutApp.Server.Models
{
    public class GameModel
    {
        public String GameSessionId { get; set; }
        public TeamModel RedTeam { get; set; }
        public TeamModel BlueTeam { get; set; }
        public int TotalRounds { get; private set; } = 8;
        public List<RoundModel> Rounds { get; private set; }
        public int CurrentRoundIndex { get; private set; }
        public RoundModel CurrentRound => Rounds[CurrentRoundIndex];

        public GameModel()
        {
            RedTeam = new TeamModel();
            BlueTeam = new TeamModel();
            Rounds = new List<RoundModel>(TotalRounds);
            InitializeRounds();
        }

        private void InitializeRounds()
        {
            for (int i = 0; i < TotalRounds; i++)
            {
                Rounds.Add(new RoundModel(i + 1,GameSessionId));
            }
        }

        /*public void StartNextRound()
        {
            if (CurrentRoundIndex < TotalRounds - 1)
            {
                CurrentRoundIndex++;
                // Additional logic to start the round (e.g., set CurrentPainter, ActiveWord, etc.)
            }
            else
            {
                // Handle end of the game
            }
        }*/

        // Additional methods to handle game logic, score updates, etc.
    }
}