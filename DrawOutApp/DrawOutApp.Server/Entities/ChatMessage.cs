using DrawOutApp.Server.Models;
using System.ComponentModel.DataAnnotations;

namespace DrawOutApp.Server.Entities
{
    public class ChatMessage
    {
        public string? Sender { get; set; }
        [MaxLength(100)]
        public string? Content { get; set; }
        public long Timestamp { get; set; }

        public ChatMessage() { }

        public ChatMessage(ChatMsgRecord message)
        {
            Sender = message.Sender;
            Content = message.Content;
            Timestamp = message.Timestamp;
        }
    }
    public record class ChatMsgRecord(string Sender, string Content, long Timestamp);
}
