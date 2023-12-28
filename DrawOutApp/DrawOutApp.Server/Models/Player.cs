using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using System.Collections;

namespace DrawOutApp.Server.Models
{
    public enum Role { Player, Painter, RoomAdmin, TeamLeader }
    public class Player
    {
        
        public String SessionId { get; set; } = null!; //kljuc

        public String? GameSessionId { get; set; }

        public String? Nickname { get; set; }

        public HashSet<Role> Roles { get; set; }

        public String? Icon { get; set; } //na kraj

        public int TeamId { get; set; } = 0;

        public Player() 
        {
            Roles = new HashSet<Role>();
        }
    }
}
