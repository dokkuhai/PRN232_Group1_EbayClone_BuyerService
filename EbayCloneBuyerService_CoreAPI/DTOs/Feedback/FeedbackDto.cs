namespace EbayCloneBuyerService_CoreAPI.DTOs.Feedback
{
    public class FeedbackDto
    {
        public int Id { get; set; }

        // Order & Transaction info
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }

        // Buyer info
        public int BuyerId { get; set; }
        public string BuyerUsername { get; set; }
        public string BuyerAvatar { get; set; }

        // Seller info
        public int SellerId { get; set; }
        public string SellerUsername { get; set; }

        // Product info
        public int ProductId { get; set; }
        public string ProductTitle { get; set; }
        public string ProductImage { get; set; }

        // Feedback content
        public string FeedbackType { get; set; }  // "positive", "neutral", "negative"
        public string Comment { get; set; }

        // Detailed ratings (1-5 stars)
        public int ItemDescriptionRating { get; set; }
        public int CommunicationRating { get; set; }
        public int ShippingSpeedRating { get; set; }
        public int ShippingCostRating { get; set; }

        // Metadata
        public bool IsVerifiedPurchase { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsEdited { get; set; }
    }
}