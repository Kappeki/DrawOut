using DrawOutApp.Server.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace DrawOutApp.Server.Entities
{
    public enum Role { Player, Painter, RoomAdmin, TeamLeader, Blue, Red }
    public class User
    {
        public string _sessionKey { get; set; } = null!;
        public string? Nickname { get; set; }
        public string? Icon { get; set; }
        public HashSet<Role>? Roles { get; set; }
        public User()
        {
            _sessionKey = $"user:{Guid.NewGuid()}";
            Roles = new HashSet<Role>();
        }
        public string SerializeRoles()
        {
            return Roles != null ? string.Join(",", Roles) : string.Empty;
        }
        public void DeserializeRoles(string rolesString)
        {
            if (!string.IsNullOrEmpty(rolesString))
            {
                Roles = new HashSet<Role>(rolesString.Split(',').Select(r => Enum.Parse<Role>(r)));
            }
            else
            {
                Roles = new HashSet<Role>();
            }
        }
    }
}
