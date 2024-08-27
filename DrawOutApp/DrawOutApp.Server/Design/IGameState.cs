using DrawOutApp.Server.Entities;

namespace DrawOutApp.Server.Design
{
    public interface IGameState
    {
        string _name { get; }
        void StartRound(Game game);
        void EndRound(Game game);
        void CheckWord(Game game, string word);
    }
}
