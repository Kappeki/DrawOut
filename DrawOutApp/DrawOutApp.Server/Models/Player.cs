namespace DrawOutApp.Server.Models
{
    public class Player
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public string Username { get; set; } = String.Empty;
    }
}
