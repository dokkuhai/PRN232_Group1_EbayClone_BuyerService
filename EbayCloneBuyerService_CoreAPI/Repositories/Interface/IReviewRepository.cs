using EbayCloneBuyerService_CoreAPI.DTOs.Review;
using EbayCloneBuyerService_CoreAPI.Models;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Interface
{
    public interface IReviewRepository
    {
        /// <summary>
        /// Lấy review theo ID
        /// </summary>
        Task<Review> GetByIdAsync(int id);

        /// <summary>
        /// Lấy tất cả reviews với filter, sort, pagination
        /// </summary>
        Task<(IEnumerable<Review> Reviews, int TotalCount)> GetAllAsync(ReviewQueryParams queryParams);

        /// <summary>
        /// Lấy reviews của 1 sản phẩm
        /// </summary>
        Task<IEnumerable<Review>> GetByProductIdAsync(int productId);

        /// <summary>
        /// Lấy reviews của 1 user
        /// </summary>
        Task<IEnumerable<Review>> GetByReviewerIdAsync(int reviewerId);

        /// <summary>
        /// Check xem user đã review sản phẩm này chưa
        /// </summary>
        Task<bool> HasUserReviewedProductAsync(int userId, int productId);

        /// <summary>
        /// Check xem user đã mua sản phẩm này chưa (để validate review)
        /// </summary>
        Task<bool> HasUserPurchasedProductAsync(int userId, int productId);

        /// <summary>
        /// Tạo review mới
        /// </summary>
        Task<Review> CreateAsync(Review review);

        /// <summary>
        /// Update review
        /// </summary>
        Task<Review> UpdateAsync(Review review);

        /// <summary>
        /// Xóa review
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Tính average rating của sản phẩm
        /// </summary>
        Task<double> GetAverageRatingAsync(int productId);

        /// <summary>
        /// Đếm số lượng reviews của sản phẩm
        /// </summary>
        Task<int> GetReviewCountAsync(int productId);
    }
}





