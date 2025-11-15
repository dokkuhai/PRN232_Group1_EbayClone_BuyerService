namespace EbayCloneBuyerService_CoreAPI.DTOs.Message
{
    public class MessageThreadDto
    {
        // Other user info
        public int OtherUserId { get; set; }
        public string OtherUsername { get; set; }
        public string OtherAvatar { get; set; }

        // Thread info
        public int UnreadCount { get; set; }
        public DateTime LastMessageTime { get; set; }
        public string LastMessagePreview { get; set; }

        // Messages in thread
        public List<MessageDto> Messages { get; set; } = new();
    }
}
