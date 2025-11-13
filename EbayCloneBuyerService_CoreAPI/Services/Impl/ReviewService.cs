using EbayCloneBuyerService_CoreAPI.DTOs.Review;
using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using EbayCloneBuyerService_CoreAPI.Services.Interface;

namespace EbayCloneBuyerService_CoreAPI.Services.Impl
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepo;
        private readonly INotificationService _notificationService;

        public ReviewService(IReviewRepository reviewRepo, INotificationService notificationService)
        {
            _reviewRepo = reviewRepo;
            _notificationService = notificationService;
        }

        public async Task<ReviewResponse> GetByIdAsync(int id, int? currentUserId = null)
        {
            var review = await _reviewRepo.GetByIdAsync(id);
            if (review == null) return null;

            return MapToResponse(review, currentUserId);
        }

        public async Task<(IEnumerable<ReviewResponse> Reviews, int TotalCount, int TotalPages)> GetAllAsync(
            ReviewQueryParams queryParams, int? currentUserId = null)
        {
            var (reviews, totalCount) = await _reviewRepo.GetAllAsync(queryParams);
            var totalPages = (int)Math.Ceiling((double)totalCount / queryParams.PageSize);

            var responses = reviews.Select(r => MapToResponse(r, currentUserId));
            return (responses, totalCount, totalPages);
        }

        public async Task<IEnumerable<ReviewResponse>> GetByProductIdAsync(int productId, int? currentUserId = null)
        {
            var reviews = await _reviewRepo.GetByProductIdAsync(productId);
            return reviews.Select(r => MapToResponse(r, currentUserId));
        }

        public async Task<IEnumerable<ReviewResponse>> GetByUserIdAsync(int userId, int? currentUserId = null)
        {
            var reviews = await _reviewRepo.GetByReviewerIdAsync(userId);
            return reviews.Select(r => MapToResponse(r, currentUserId));
        }

        public async Task<ReviewResponse> CreateAsync(int userId, CreateReviewRequest request)
        {
            // === VALIDATION 1: User đã review sản phẩm này chưa? ===
            if (await _reviewRepo.HasUserReviewedProductAsync(userId, request.ProductId))
            {
                throw new InvalidOperationException("You have already reviewed this product");
            }

            // === VALIDATION 2: User đã mua sản phẩm này chưa? ===
            // TẮT validation này nếu muốn cho phép review tự do (như Amazon)
            // if (!await _reviewRepo.HasUserPurchasedProductAsync(userId, request.ProductId))
            // {
            //     throw new InvalidOperationException("You can only review products you have purchased");
            // }

            // === CREATE REVIEW ===
            var review = new Review
            {
                ProductId = request.ProductId,
                ReviewerId = userId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.Now
            };

            var created = await _reviewRepo.CreateAsync(review);

            // === TRIGGER NOTIFICATION to Seller ===
            try
            {
                if (created.Product?.SellerId != null)
                {
                    await _notificationService.NotifyNewReviewAsync(
                        sellerId: created.Product.SellerId.Value,
                        productId: request.ProductId,
                        productTitle: created.Product.Title,
                        rating: request.Rating,
                        reviewerName: created.Reviewer?.Username ?? "Anonymous"
                    );
                }
            }
            catch (Exception ex)
            {
                // Log error nhưng không fail transaction
                Console.WriteLine($"Failed to send review notification: {ex.Message}");
            }

            return MapToResponse(created, userId);
        }

        public async Task<ReviewResponse> UpdateAsync(int reviewId, int userId, UpdateReviewRequest request, bool isAdmin = false)
        {
            var review = await _reviewRepo.GetByIdAsync(reviewId);
            if (review == null)
            {
                throw new KeyNotFoundException("Review not found");
            }

            // === AUTHORIZATION: Chỉ owner hoặc admin mới update được ===
            if (!isAdmin && review.ReviewerId != userId)
            {
                throw new UnauthorizedAccessException("You can only update your own reviews");
            }

            // === UPDATE ===
            if (request.Rating.HasValue)
            {
                review.Rating = request.Rating.Value;
            }

            if (!string.IsNullOrWhiteSpace(request.Comment))
            {
                review.Comment = request.Comment;
            }

            var updated = await _reviewRepo.UpdateAsync(review);
            return MapToResponse(updated, userId);
        }

        public async Task<bool> DeleteAsync(int reviewId, int userId, bool isAdmin = false)
        {
            var review = await _reviewRepo.GetByIdAsync(reviewId);
            if (review == null) return false;

            // === AUTHORIZATION ===
            if (!isAdmin && review.ReviewerId != userId)
            {
                throw new UnauthorizedAccessException("You can only delete your own reviews");
            }

            return await _reviewRepo.DeleteAsync(reviewId);
        }

        public async Task<ProductReviewStats> GetProductStatsAsync(int productId)
        {
            var reviews = await _reviewRepo.GetByProductIdAsync(productId);
            var reviewList = reviews.ToList();

            var stats = new ProductReviewStats
            {
                ProductId = productId,
                TotalReviews = reviewList.Count,
                AverageRating = reviewList.Any() ? reviewList.Average(r => r.Rating ?? 0) : 0,
                RatingDistribution = new Dictionary<int, int>
                {
                    { 1, reviewList.Count(r => r.Rating == 1) },
                    { 2, reviewList.Count(r => r.Rating == 2) },
                    { 3, reviewList.Count(r => r.Rating == 3) },
                    { 4, reviewList.Count(r => r.Rating == 4) },
                    { 5, reviewList.Count(r => r.Rating == 5) }
                }
            };

            return stats;
        }

        /// <summary>
        /// Helper: Map Review entity sang ReviewResponse DTO
        /// </summary>
        private ReviewResponse MapToResponse(Review review, int? currentUserId)
        {
            return new ReviewResponse
            {
                Id = review.Id,
                ProductId = review.ProductId ?? 0,
                ProductTitle = review.Product?.Title ?? "Unknown Product",
                ReviewerId = review.ReviewerId ?? 0,
                ReviewerName = review.Reviewer?.Username ?? "Anonymous",
                ReviewerAvatar = review.Reviewer?.AvatarUrl,
                Rating = review.Rating ?? 0,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt ?? DateTime.Now,
                IsOwner = currentUserId.HasValue && review.ReviewerId == currentUserId.Value
            };
        }
    }
}