namespace DrawOutApp.Server.Entities
{
    public class ChatMessage
    {
        public string ChatMessageId { get; set; }
        public String SenderId { get; set; }
        public String Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public ChatMessage()
        {
            ChatMessageId = $"chatMessage:{SenderId}:{Guid.NewGuid().ToString()}";
        }
    }
}
