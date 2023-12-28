using DrawOutApp.Server.Models;
using System.ComponentModel.DataAnnotations;

namespace DrawOutApp.Server.Entities
{
    public class ChatMessage
    {
        [Required]
        public String Sender { get; set; }
        [Required]
        [MaxLength(100)]
        public String Content { get; set; }
        [Required]
        public long Timestamp { get; set; } // Use Unix timestamp for simplicity

        public ChatMessage() { }

        public ChatMessage(ChatMessageModel message)
        {
            Sender = message.Sender;
            Content = message.Content;
            Timestamp = new DateTimeOffset(message.Timestamp).ToUnixTimeSeconds();
        }

        public ChatMessageModel ToBusinessModel()
        {
            return new ChatMessageModel
            {
                Sender = this.Sender,
                Content = this.Content,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(this.Timestamp).DateTime
            };
        }
    }
}
