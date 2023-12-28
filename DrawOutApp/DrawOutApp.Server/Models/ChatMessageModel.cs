namespace DrawOutApp.Server.Models
{
    public class ChatMessageModel
    { 
        public String Sender { get; set; }

        public String Content { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
