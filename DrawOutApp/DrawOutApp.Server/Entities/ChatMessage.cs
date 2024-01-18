using DrawOutApp.Server.Models;
using System.ComponentModel.DataAnnotations;

namespace DrawOutApp.Server.Entities
{
    public class ChatMessage
    {
        public String Sender { get; set; }
        [MaxLength(100)]
        public String Content { get; set; }
        public long Timestamp { get; set; }

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
