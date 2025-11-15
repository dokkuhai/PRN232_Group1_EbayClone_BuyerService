using System.ComponentModel.DataAnnotations;

namespace EbayCloneBuyerService_CoreAPI.DTOs.Feedback
{
    public class UpdateFeedbackDto
    {
        [RegularExpression("^(positive|neutral|negative)$")]
        public string? FeedbackType { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }

        [Range(1, 5)]
        public int? ItemDescriptionRating { get; set; }

        [Range(1, 5)]
        public int? CommunicationRating { get; set; }

        [Range(1, 5)]
        public int? ShippingSpeedRating { get; set; }

        [Range(1, 5)]
        public int? ShippingCostRating { get; set; }
    }
}
