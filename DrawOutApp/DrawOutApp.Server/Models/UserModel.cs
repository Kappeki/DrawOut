using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace DrawOutApp.Server.Models
{
    public enum Role { Player, Painter, RoomAdmin, TeamLeader }
    public class UserModel
    {
        public String SessionId { get; set; }

        public String? GameSessionId { get; set; }

        public String? Nickname { get; set; }

        public HashSet<Role> Roles { get; set; } 

        public String? Icon { get; set; } //na kraj

        public int TeamId { get; set; } = 0;

        public UserModel() 
        {
            Roles = new HashSet<Role>();
        }
    }
}
