namespace EbayCloneBuyerService_CoreAPI.DTOs.Message
{
    public class MessageDto
    {
        public int Id { get; set; }

        // Sender info
        public int SenderId { get; set; }
        public string SenderUsername { get; set; }
        public string SenderAvatar { get; set; }

        // Receiver info
        public int ReceiverId { get; set; }
        public string ReceiverUsername { get; set; }

        // Message content
        public string Content { get; set; }
        public string MessageType { get; set; }  // "general", "order", "product_inquiry"

        // Related entities
        public int? OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public int? ProductId { get; set; }
        public string? ProductTitle { get; set; }

        // Status
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
