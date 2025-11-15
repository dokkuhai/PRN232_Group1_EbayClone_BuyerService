using EbayCloneBuyerService_CoreAPI.DTOs.Review;

namespace EbayCloneBuyerService_CoreAPI.Services.Interface
{
    public interface IReviewService
    {
        /// <summary>
        /// Lấy review theo ID
        /// </summary>
        Task<ReviewResponse> GetByIdAsync(int id, int? currentUserId = null);

        /// <summary>
        /// Lấy danh sách reviews với filter, sort, pagination
        /// </summary>
        Task<(IEnumerable<ReviewResponse> Reviews, int TotalCount, int TotalPages)> GetAllAsync(
            ReviewQueryParams queryParams, int? currentUserId = null);

        /// <summary>
        /// Lấy reviews của 1 sản phẩm
        /// </summary>
        Task<IEnumerable<ReviewResponse>> GetByProductIdAsync(int productId, int? currentUserId = null);

        /// <summary>
        /// Lấy reviews của 1 user
        /// </summary>
        Task<IEnumerable<ReviewResponse>> GetByUserIdAsync(int userId, int? currentUserId = null);

        /// <summary>
        /// Tạo review mới (có validation)
        /// </summary>
        Task<ReviewResponse> CreateAsync(int userId, CreateReviewRequest request);

        /// <summary>
        /// Update review (chỉ owner hoặc admin)
        /// </summary>
        Task<ReviewResponse> UpdateAsync(int reviewId, int userId, UpdateReviewRequest request, bool isAdmin = false);

        /// <summary>
        /// Xóa review (chỉ owner hoặc admin)
        /// </summary>
        Task<bool> DeleteAsync(int reviewId, int userId, bool isAdmin = false);

        /// <summary>
        /// Lấy statistics của sản phẩm (average rating, total reviews)
        /// </summary>
        Task<ProductReviewStats> GetProductStatsAsync(int productId);
    }

    /// <summary>
    /// Statistics DTO cho review của sản phẩm
    /// </summary>
    public class ProductReviewStats
    {
        public int ProductId { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } // {1: 5, 2: 10, 3: 20, 4: 30, 5: 35}
    }
}




