using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace DrawOutApp.Server.Models
{
    public enum Role { Player, Painter, RoomAdmin, TeamLeader }
    public class UserModel
    {
        public string? SessionId { get; set; }

        public string? GameSessionId { get; set; }

        public string? Nickname { get; set; }

        public HashSet<Role> Roles { get; set; } 

        public string? Icon { get; set; } //na kraj

        public string? TeamId { get; set; }
        public UserModel() 
        {
            Roles = new HashSet<Role>();
        }
    }
}
