namespace EbayCloneBuyerService_CoreAPI.DTOs.Feedback
{
    public class CanLeaveFeedbackDto
    {
        public int OrderId { get; set; }
        public bool CanLeave { get; set; }
        public string Reason { get; set; }  // "not_delivered", "already_left", "too_late", "ok"

        // Order info
        public string OrderNumber { get; set; }
        public string OrderStatus { get; set; }
        public DateTime? DeliveredAt { get; set; }

        // Existing feedback info (if any)
        public int? ExistingFeedbackId { get; set; }
        public string? ExistingFeedbackType { get; set; }
        public DateTime? FeedbackCreatedAt { get; set; }
        public bool CanEdit { get; set; }  // Within 60 days
    }
}
