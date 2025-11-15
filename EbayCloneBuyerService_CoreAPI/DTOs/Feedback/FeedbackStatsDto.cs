namespace EbayCloneBuyerService_CoreAPI.DTOs.Feedback
{
    public class FeedbackStatsDto
    {
        // Seller info
        public int SellerId { get; set; }
        public string SellerUsername { get; set; }
        public string SellerAvatar { get; set; }

        // Overall statistics
        public int TotalFeedbacks { get; set; }
        public int PositiveCount { get; set; }
        public int NeutralCount { get; set; }
        public int NegativeCount { get; set; }
        public decimal PositiveRate { get; set; }  // Percentage: 98.5%

        // Detailed ratings averages (1-5 stars)
        public decimal AvgItemDescription { get; set; }
        public decimal AvgCommunication { get; set; }
        public decimal AvgShippingSpeed { get; set; }
        public decimal AvgShippingCost { get; set; }

        // Recent feedbacks (last 10)
        public List<FeedbackDto> RecentFeedbacks { get; set; } = new();

        // Time period stats
        public FeedbackPeriodStatsDto Last12Months { get; set; }
        public FeedbackPeriodStatsDto Last6Months { get; set; }
        public FeedbackPeriodStatsDto Last30Days { get; set; }
    }

    public class FeedbackPeriodStatsDto
    {
        public int TotalFeedbacks { get; set; }
        public int PositiveCount { get; set; }
        public int NeutralCount { get; set; }
        public int NegativeCount { get; set; }
        public decimal PositiveRate { get; set; }
    }
}
