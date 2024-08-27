using DrawOutApp.Server.Entities;

namespace DrawOutApp.Server.Design
{
    public class WaitingForPlayers : IGameState
    {
        public string _name => "WaitingForPlayers";
        public void StartRound(Game game)
        {
            // do nothing
        }
        public void EndRound(Game game)
        {
            // do nothing
        }
        public void CheckWord(Game game, string word)
        {
           // do nothing
        }
    }
}
