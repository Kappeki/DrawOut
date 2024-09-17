namespace DrawOutApp.Server.Games.Contracts
{
    public interface IGameCommand
    {
        Task ExecuteAsync();
    }
}
