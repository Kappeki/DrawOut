using DrawOutApp.Server.Entities;

namespace DrawOutApp.Server.Design
{
    public class Completed : IGameState
    {
        public string _name => "Completed";
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
