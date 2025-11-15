using System.ComponentModel.DataAnnotations;

namespace EbayCloneBuyerService_CoreAPI.DTOs.Review
{
    public class UpdateReviewRequest
    {
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int? Rating { get; set; }

        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Comment must be between 10 and 1000 characters")]
        public string Comment { get; set; }
    }
}




