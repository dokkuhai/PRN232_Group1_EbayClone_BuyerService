using EbayCloneBuyerService_CoreAPI.DTOs.Review;
using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Impl
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly CloneEbayDbContext _context;

        public ReviewRepository(CloneEbayDbContext context)
        {
            _context = context;
        }

        public async Task<Review> GetByIdAsync(int id)
        {
            return await _context.Reviews
                .Include(r => r.Product)
                .Include(r => r.Reviewer)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<(IEnumerable<Review> Reviews, int TotalCount)> GetAllAsync(ReviewQueryParams queryParams)
        {
            var query = _context.Reviews
                .Include(r => r.Product)
                .Include(r => r.Reviewer)
                .AsQueryable();

            // === FILTERING (OData-like) ===
            if (queryParams.ProductId.HasValue)
            {
                query = query.Where(r => r.ProductId == queryParams.ProductId.Value);
            }

            if (queryParams.ReviewerId.HasValue)
            {
                query = query.Where(r => r.ReviewerId == queryParams.ReviewerId.Value);
            }

            if (queryParams.Rating.HasValue)
            {
                query = query.Where(r => r.Rating == queryParams.Rating.Value);
            }

            if (queryParams.MinRating.HasValue)
            {
                query = query.Where(r => r.Rating >= queryParams.MinRating.Value);
            }

            if (queryParams.MaxRating.HasValue)
            {
                query = query.Where(r => r.Rating <= queryParams.MaxRating.Value);
            }

            if (!string.IsNullOrWhiteSpace(queryParams.SearchKeyword))
            {
                query = query.Where(r => r.Comment.Contains(queryParams.SearchKeyword));
            }

            // === TOTAL COUNT (before pagination) ===
            var totalCount = await query.CountAsync();

            // === SORTING ===
            query = queryParams.SortBy?.ToLower() switch
            {
                "rating" => query.OrderBy(r => r.Rating),
                "rating_desc" => query.OrderByDescending(r => r.Rating),
                "createdat" => query.OrderBy(r => r.CreatedAt),
                "createdat_desc" or _ => query.OrderByDescending(r => r.CreatedAt)
            };

            // === PAGINATION ===
            var pageSize = Math.Clamp(queryParams.PageSize, 1, 100);
            var page = Math.Max(queryParams.Page, 1);
            var skip = (page - 1) * pageSize;

            var reviews = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return (reviews, totalCount);
        }

        public async Task<IEnumerable<Review>> GetByProductIdAsync(int productId)
        {
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByReviewerIdAsync(int reviewerId)
        {
            return await _context.Reviews
                .Include(r => r.Product)
                .Where(r => r.ReviewerId == reviewerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasUserReviewedProductAsync(int userId, int productId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.ReviewerId == userId && r.ProductId == productId);
        }

        public async Task<bool> HasUserPurchasedProductAsync(int userId, int productId)
        {
            // Check nếu user đã mua sản phẩm này (có trong OrderItem của Order đã hoàn thành)
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .AnyAsync(oi => oi.Order.BuyerId == userId
                            && oi.ProductId == productId
                            && oi.Order.Status == "COMPLETED"); // Hoặc "DELIVERED"
        }

        public async Task<Review> CreateAsync(Review review)
        {
            review.CreatedAt = DateTime.Now;
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();

            // Load navigation properties
            await _context.Entry(review).Reference(r => r.Product).LoadAsync();
            await _context.Entry(review).Reference(r => r.Reviewer).LoadAsync();

            return review;
        }

        public async Task<Review> UpdateAsync(Review review)
        {
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            return await GetByIdAsync(review.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return false;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<double> GetAverageRatingAsync(int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .ToListAsync();

            return reviews.Any() ? reviews.Average(r => r.Rating ?? 0) : 0;
        }

        public async Task<int> GetReviewCountAsync(int productId)
        {
            return await _context.Reviews
                .CountAsync(r => r.ProductId == productId);
        }
    }
}




