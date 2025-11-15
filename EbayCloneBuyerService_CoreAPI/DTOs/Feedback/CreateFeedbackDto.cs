using System.ComponentModel.DataAnnotations;

namespace EbayCloneBuyerService_CoreAPI.DTOs.Feedback
{
    public class CreateFeedbackDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public int SellerId { get; set; }

        [Required]
        public int ProductId { get; set; }

        // Overall feedback
        [Required]
        [RegularExpression("^(positive|neutral|negative)$",
            ErrorMessage = "FeedbackType must be 'positive', 'neutral', or 'negative'")]
        public string FeedbackType { get; set; }

        // Optional comment
        [MaxLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }

        // Detailed ratings (1-5 stars) - REQUIRED
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int ItemDescriptionRating { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int CommunicationRating { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int ShippingSpeedRating { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int ShippingCostRating { get; set; }
    }
}
