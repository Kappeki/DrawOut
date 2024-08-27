using DrawOutApp.Server.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Security.Permissions;
using System.Text.Json.Serialization;

namespace DrawOutApp.Server.Models
{
    public record class RoomRequest(string RoomName, string? Password);
    public record class JoinRoomRequest(string? RoomId, string? RoomUrl, string? Password); 


    public class RoomListItem
    {
        public string RoomId { get; init; } = null!;  // Read-only, set during mapping
        public string RoomName { get; set; } = null!;
        public bool HasPassword { get; set; }         // Indicates if the room has a password
        public string GameState { get; set; } = null!;
        public int PlayerCount { get; set; }
    }

    public class RoomModel
    {
        public string RoomName { get; set; } = null!;
        public string? PasswordHash { get; set; }
        public string? RoomURL { get; set; }
        public int PlayerCount { get; set; }
        public string RoomAdminId { get; set; } = null!;
        public List<string>? Players { get; set; }
        public List<string>? CustomWords { get; set; }
        public string? SelectedWordPack { get; set; }
        public string GameState { get; set; } = null!;
        public int RoundTime { get; set; }
        public DateTime TimeElapsed { get; set; }
    }
}
