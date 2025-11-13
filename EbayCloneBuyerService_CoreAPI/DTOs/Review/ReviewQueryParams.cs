namespace EbayCloneBuyerService_CoreAPI.DTOs.Review
{
    public class ReviewQueryParams
    {
        /// <summary>
        /// Filter theo ProductId
        /// </summary>
        public int? ProductId { get; set; }

        /// <summary>
        /// Filter theo ReviewerId (User)
        /// </summary>
        public int? ReviewerId { get; set; }

        /// <summary>
        /// Filter theo Rating (exact match)
        /// </summary>
        public int? Rating { get; set; }

        /// <summary>
        /// Filter theo MinRating (>=)
        /// </summary>
        public int? MinRating { get; set; }

        /// <summary>
        /// Filter theo MaxRating (<=)
        /// </summary>
        public int? MaxRating { get; set; }

        /// <summary>
        /// Search keyword trong comment
        /// </summary>
        public string SearchKeyword { get; set; }

        /// <summary>
        /// Sort by: "rating", "createdAt", "rating_desc", "createdAt_desc"
        /// </summary>
        public string SortBy { get; set; } = "createdAt_desc";

        /// <summary>
        /// Page number (1-based)
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Page size (max 100)
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}




