using DrawOutApp.Server.Entities;

namespace DrawOutApp.Server.Design
{
    public class InProgress : IGameState
    {
        public string _name => "InProgress";
        public void StartRound(Game game)
        {
            if(game.CurrentRound < game.TotalRounds)
            {
                game.CurrentRound++;
                game.SetState(new InProgress());
            }
            else
            {
                game.SetState(new Completed());
            }
        }
        public void EndRound(Game game)
        {
            
        }
        public void CheckWord(Game game, string word)
        {
            // do nothing
        }
    }
}
