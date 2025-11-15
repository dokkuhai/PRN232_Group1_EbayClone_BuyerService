using System.Text.Json;
using EbayCloneBuyerService_CoreAPI.DTOs.Feedback;
using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using EbayCloneBuyerService_CoreAPI.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace EbayCloneBuyerService_CoreAPI.Services.Impl;

/// <summary>
/// FeedbackService v4 - Fixed all type conversion and JsonDocument errors
/// </summary>
public class FeedbackService : IFeedbackService
{
    private readonly IGenericRepository<Review> _reviewRepo;
    private readonly IGenericRepository<Feedback> _feedbackRepo;
    private readonly IGenericRepository<OrderTable> _orderRepo;
    private readonly IGenericRepository<Product> _productRepo;
    private readonly IGenericRepository<User> _userRepo;
    private readonly CloneEbayDbContext _context;

    public FeedbackService(
        IGenericRepository<Review> reviewRepo,
        IGenericRepository<Feedback> feedbackRepo,
        IGenericRepository<OrderTable> orderRepo,
        IGenericRepository<Product> productRepo,
        IGenericRepository<User> userRepo,
        CloneEbayDbContext context)
    {
        _reviewRepo = reviewRepo;
        _feedbackRepo = feedbackRepo;
        _orderRepo = orderRepo;
        _productRepo = productRepo;
        _userRepo = userRepo;
        _context = context;
    }

    // ============================================================
    // CREATE
    // ============================================================

    public async Task<FeedbackDto> LeaveFeedbackAsync(int buyerId, CreateFeedbackDto dto)
    {
        // 1. Validate order
        var order = await _orderRepo.GetByIdAsync(dto.OrderId);
        if (order == null)
            throw new Exception("Order not found");

        if (order.BuyerId != buyerId)
            throw new Exception("You can only leave feedback for your own orders");

        if (order.Status != "Delivered")
            throw new Exception("You can only leave feedback after order is delivered");

        // 2. Check if feedback already exists
        var existingFeedback = await _context.Reviews
            .FirstOrDefaultAsync(r => r.ProductId == dto.ProductId
                                   && r.ReviewerId == buyerId
                                   && r.Comment != null
                                   && r.Comment.Contains("\"feedbackType\""));

        if (existingFeedback != null)
            throw new Exception("You have already left feedback for this order");

        // 3. Create feedback data structure
        var feedbackData = new
        {
            feedbackType = dto.FeedbackType,
            comment = dto.Comment ?? "",
            ratings = new
            {
                itemDescription = dto.ItemDescriptionRating,
                communication = dto.CommunicationRating,
                shippingSpeed = dto.ShippingSpeedRating,
                shippingCost = dto.ShippingCostRating
            },
            orderId = dto.OrderId,
            sellerId = dto.SellerId,
            isVerifiedPurchase = true
        };

        // 4. Calculate overall rating
        var overallRating = (dto.ItemDescriptionRating +
                            dto.CommunicationRating +
                            dto.ShippingSpeedRating +
                            dto.ShippingCostRating) / 4.0;

        // 5. Create Review record
        var review = new Review
        {
            ProductId = dto.ProductId,
            ReviewerId = buyerId,
            Rating = (int)Math.Round(overallRating),
            Comment = JsonSerializer.Serialize(feedbackData),
            CreatedAt = DateTime.UtcNow
        };

        await _reviewRepo.AddAsync(review);

        // 6. Update seller's feedback summary
        await UpdateSellerFeedbackSummaryAsync(dto.SellerId);

        // 7. Map to DTO and return
        return await MapToFeedbackDto(review, dto.SellerId);
    }

    // ============================================================
    // READ
    // ============================================================

    public async Task<FeedbackStatsDto> GetSellerFeedbackStatsAsync(int sellerId)
    {
        // Get seller info
        var seller = await _userRepo.GetByIdAsync(sellerId);
        if (seller == null)
            throw new Exception("Seller not found");

        // Get all feedback reviews for this seller
        var feedbacks = await GetSellerFeedbackReviewsAsync(sellerId);

        // Parse feedback data
        var feedbackDtos = new List<FeedbackDto>();
        foreach (var review in feedbacks)
        {
            try
            {
                var dto = await MapToFeedbackDto(review, sellerId);
                feedbackDtos.Add(dto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing feedback: {ex.Message}");
            }
        }

        // Calculate statistics
        var totalFeedbacks = feedbackDtos.Count;
        var positiveCount = feedbackDtos.Count(f => f.FeedbackType == "positive");
        var neutralCount = feedbackDtos.Count(f => f.FeedbackType == "neutral");
        var negativeCount = feedbackDtos.Count(f => f.FeedbackType == "negative");

        var positiveRate = totalFeedbacks > 0
            ? (decimal)positiveCount / totalFeedbacks * 100m
            : 0m;

        // Calculate detailed ratings averages (FIX: Explicit cast to decimal)
        var avgItemDesc = feedbackDtos.Any()
            ? (decimal)feedbackDtos.Average(f => (double)f.ItemDescriptionRating)
            : 0m;
        var avgComm = feedbackDtos.Any()
            ? (decimal)feedbackDtos.Average(f => (double)f.CommunicationRating)
            : 0m;
        var avgShipSpeed = feedbackDtos.Any()
            ? (decimal)feedbackDtos.Average(f => (double)f.ShippingSpeedRating)
            : 0m;
        var avgShipCost = feedbackDtos.Any()
            ? (decimal)feedbackDtos.Average(f => (double)f.ShippingCostRating)
            : 0m;

        // Get recent feedbacks
        var recentFeedbacks = feedbackDtos
            .OrderByDescending(f => f.CreatedAt)
            .Take(10)
            .ToList();

        // Calculate period stats
        var now = DateTime.UtcNow;
        var last12Months = CalculatePeriodStats(feedbackDtos, now.AddMonths(-12));
        var last6Months = CalculatePeriodStats(feedbackDtos, now.AddMonths(-6));
        var last30Days = CalculatePeriodStats(feedbackDtos, now.AddDays(-30));

        return new FeedbackStatsDto
        {
            SellerId = sellerId,
            SellerUsername = seller.Username ?? "",
            SellerAvatar = seller.AvatarUrl ?? "",
            TotalFeedbacks = totalFeedbacks,
            PositiveCount = positiveCount,
            NeutralCount = neutralCount,
            NegativeCount = negativeCount,
            PositiveRate = Math.Round(positiveRate, 1),
            AvgItemDescription = Math.Round(avgItemDesc, 1),
            AvgCommunication = Math.Round(avgComm, 1),
            AvgShippingSpeed = Math.Round(avgShipSpeed, 1),
            AvgShippingCost = Math.Round(avgShipCost, 1),
            RecentFeedbacks = recentFeedbacks,
            Last12Months = last12Months,
            Last6Months = last6Months,
            Last30Days = last30Days
        };
    }

    public async Task<List<FeedbackDto>> GetSellerFeedbacksAsync(int sellerId, int page = 1, int pageSize = 20)
    {
        var feedbacks = await GetSellerFeedbackReviewsAsync(sellerId);

        var feedbackDtos = new List<FeedbackDto>();
        foreach (var review in feedbacks.Skip((page - 1) * pageSize).Take(pageSize))
        {
            try
            {
                var dto = await MapToFeedbackDto(review, sellerId);
                feedbackDtos.Add(dto);
            }
            catch { /* Skip invalid */ }
        }

        return feedbackDtos.OrderByDescending(f => f.CreatedAt).ToList();
    }

    public async Task<List<FeedbackDto>> GetBuyerFeedbacksAsync(int buyerId)
    {
        var feedbacks = await _context.Reviews
            .Where(r => r.ReviewerId == buyerId
                     && r.Comment != null
                     && r.Comment.Contains("\"feedbackType\""))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var feedbackDtos = new List<FeedbackDto>();
        foreach (var review in feedbacks)
        {
            try
            {
                // FIX: Use RootElement
                var feedbackData = ParseFeedbackData(review.Comment);
                var sellerId = feedbackData.RootElement.GetProperty("sellerId").GetInt32();
                var dto = await MapToFeedbackDto(review, sellerId);
                feedbackDtos.Add(dto);
            }
            catch { /* Skip invalid */ }
        }

        return feedbackDtos;
    }

    public async Task<List<FeedbackDto>> GetReceivedFeedbacksAsync(int sellerId, int page = 1, int pageSize = 20)
    {
        return await GetSellerFeedbacksAsync(sellerId, page, pageSize);
    }

    public async Task<CanLeaveFeedbackDto> CanLeaveFeedbackAsync(int buyerId, int orderId)
    {
        var order = await _context.OrderTables
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return new CanLeaveFeedbackDto
            {
                OrderId = orderId,
                CanLeave = false,
                Reason = "Order not found"
            };
        }

        if (order.BuyerId != buyerId)
        {
            return new CanLeaveFeedbackDto
            {
                OrderId = orderId,
                CanLeave = false,
                Reason = "Not your order"
            };
        }

        if (order.Status != "Delivered")
        {
            return new CanLeaveFeedbackDto
            {
                OrderId = orderId,
                CanLeave = false,
                Reason = "Order not delivered yet",
                OrderNumber = orderId.ToString(),
                OrderStatus = order.Status
            };
        }

        // Check if feedback already exists
        var firstProduct = order.OrderItems?.FirstOrDefault()?.Product;
        if (firstProduct != null)
        {
            var existingFeedback = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ProductId == firstProduct.Id
                                       && r.ReviewerId == buyerId
                                       && r.Comment != null
                                       && r.Comment.Contains("\"feedbackType\""));

            if (existingFeedback != null)
            {
                var canEdit = (DateTime.UtcNow - existingFeedback.CreatedAt.GetValueOrDefault()).TotalDays <= 60;

                return new CanLeaveFeedbackDto
                {
                    OrderId = orderId,
                    CanLeave = false,
                    Reason = "Feedback already left",
                    OrderNumber = orderId.ToString(),
                    OrderStatus = order.Status,
                    ExistingFeedbackId = existingFeedback.Id,
                    FeedbackCreatedAt = existingFeedback.CreatedAt,
                    CanEdit = canEdit
                };
            }
        }

        return new CanLeaveFeedbackDto
        {
            OrderId = orderId,
            CanLeave = true,
            Reason = "ok",
            OrderNumber = orderId.ToString(),
            OrderStatus = order.Status
        };
    }

    public async Task<FeedbackDto?> GetFeedbackByIdAsync(int feedbackId)
    {
        var review = await _reviewRepo.GetByIdAsync(feedbackId);
        if (review == null || !IsFeedbackReview(review))
            return null;

        try
        {
            // FIX: Use RootElement
            var feedbackData = ParseFeedbackData(review.Comment);
            var sellerId = feedbackData.RootElement.GetProperty("sellerId").GetInt32();
            return await MapToFeedbackDto(review, sellerId);
        }
        catch
        {
            return null;
        }
    }

    // ============================================================
    // UPDATE
    // ============================================================

    public async Task<FeedbackDto> UpdateFeedbackAsync(int buyerId, int feedbackId, UpdateFeedbackDto dto)
    {
        var review = await _reviewRepo.GetByIdAsync(feedbackId);
        if (review == null)
            throw new Exception("Feedback not found");

        if (review.ReviewerId != buyerId)
            throw new Exception("You can only update your own feedback");

        // Check 60-day limit
        var daysSinceCreated = (DateTime.UtcNow - review.CreatedAt.GetValueOrDefault()).TotalDays;
        if (daysSinceCreated > 60)
            throw new Exception("Feedback can only be edited within 60 days");

        // FIX: Parse existing data with RootElement
        var feedbackData = ParseFeedbackData(review.Comment);
        var root = feedbackData.RootElement;

        // Update fields
        var ratings = root.GetProperty("ratings");
        var updatedData = new
        {
            feedbackType = dto.FeedbackType ?? root.GetProperty("feedbackType").GetString(),
            comment = dto.Comment ?? root.GetProperty("comment").GetString(),
            ratings = new
            {
                itemDescription = dto.ItemDescriptionRating ?? ratings.GetProperty("itemDescription").GetInt32(),
                communication = dto.CommunicationRating ?? ratings.GetProperty("communication").GetInt32(),
                shippingSpeed = dto.ShippingSpeedRating ?? ratings.GetProperty("shippingSpeed").GetInt32(),
                shippingCost = dto.ShippingCostRating ?? ratings.GetProperty("shippingCost").GetInt32()
            },
            orderId = root.GetProperty("orderId").GetInt32(),
            sellerId = root.GetProperty("sellerId").GetInt32(),
            isVerifiedPurchase = true,
            isEdited = true,
            updatedAt = DateTime.UtcNow
        };

        review.Comment = JsonSerializer.Serialize(updatedData);

        // Recalculate overall rating
        var newRatings = updatedData.ratings;
        var overallRating = (newRatings.itemDescription +
                            newRatings.communication +
                            newRatings.shippingSpeed +
                            newRatings.shippingCost) / 4.0;
        review.Rating = (int)Math.Round(overallRating);

        await _reviewRepo.UpdateAsync(review);

        // Update seller summary
        var sellerId = updatedData.sellerId;
        await UpdateSellerFeedbackSummaryAsync(sellerId);

        return await MapToFeedbackDto(review, sellerId);
    }

    // ============================================================
    // DELETE
    // ============================================================

    public async Task<bool> DeleteFeedbackAsync(int buyerId, int feedbackId)
    {
        var review = await _reviewRepo.GetByIdAsync(feedbackId);
        if (review == null)
            return false;

        if (review.ReviewerId != buyerId)
            throw new Exception("You can only delete your own feedback");

        // Check 60-day limit
        var daysSinceCreated = (DateTime.UtcNow - review.CreatedAt.GetValueOrDefault()).TotalDays;
        if (daysSinceCreated > 60)
            throw new Exception("Feedback can only be deleted within 60 days");

        // FIX: Get seller ID with RootElement
        var feedbackData = ParseFeedbackData(review.Comment);
        var sellerId = feedbackData.RootElement.GetProperty("sellerId").GetInt32();

        await _reviewRepo.DeleteAsync(review);

        // Update seller summary
        await UpdateSellerFeedbackSummaryAsync(sellerId);

        return true;
    }

    // ============================================================
    // HELPER METHODS
    // ============================================================

    public async Task UpdateSellerFeedbackSummaryAsync(int sellerId)
    {
        var feedbacks = await GetSellerFeedbackReviewsAsync(sellerId);

        var feedbackDtos = new List<FeedbackDto>();
        foreach (var review in feedbacks)
        {
            try
            {
                var dto = await MapToFeedbackDto(review, sellerId);
                feedbackDtos.Add(dto);
            }
            catch { /* Skip invalid */ }
        }

        var totalReviews = feedbackDtos.Count;
        var positiveCount = feedbackDtos.Count(f => f.FeedbackType == "positive");

        // FIX: Explicit decimal conversion
        var positiveRate = totalReviews > 0
            ? (decimal)positiveCount / totalReviews * 100m
            : 0m;

        var avgRating = feedbackDtos.Any()
            ? (decimal)feedbackDtos.Average(f => (double)((f.ItemDescriptionRating + f.CommunicationRating +
                                                          f.ShippingSpeedRating + f.ShippingCostRating) / 4.0))
            : 0m;

        // Update or create Feedback summary record
        var feedbackSummary = await _context.Feedbacks
            .FirstOrDefaultAsync(f => f.SellerId == sellerId);

        if (feedbackSummary == null)
        {
            feedbackSummary = new Feedback
            {
                SellerId = sellerId,
                TotalReviews = totalReviews,
                AverageRating = avgRating,
                PositiveRate = positiveRate
            };
            await _feedbackRepo.AddAsync(feedbackSummary);
        }
        else
        {
            feedbackSummary.TotalReviews = totalReviews;
            feedbackSummary.AverageRating = avgRating;
            feedbackSummary.PositiveRate = positiveRate;
            await _feedbackRepo.UpdateAsync(feedbackSummary);
        }
    }

    private async Task<List<Review>> GetSellerFeedbackReviewsAsync(int sellerId)
    {
        // Get all products by this seller
        var sellerProducts = await _context.Products
            .Where(p => p.SellerId == sellerId)
            .Select(p => p.Id)
            .ToListAsync();

        // Get all feedback reviews for seller's products
        return await _context.Reviews
            .Where(r => sellerProducts.Contains(r.ProductId ?? 0)
                     && r.Comment != null
                     && r.Comment.Contains("\"feedbackType\""))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    private async Task<FeedbackDto> MapToFeedbackDto(Review review, int sellerId)
    {
        // FIX: Use RootElement throughout
        var feedbackData = ParseFeedbackData(review.Comment);
        var root = feedbackData.RootElement;

        // Load related data
        var buyer = await _userRepo.GetByIdAsync(review.ReviewerId ?? 0);
        var seller = await _userRepo.GetByIdAsync(sellerId);
        var product = await _productRepo.GetByIdAsync(review.ProductId ?? 0);
        var orderId = root.GetProperty("orderId").GetInt32();

        var ratings = root.GetProperty("ratings");

        return new FeedbackDto
        {
            Id = review.Id,
            OrderId = orderId,
            OrderNumber = orderId.ToString(),
            BuyerId = review.ReviewerId ?? 0,
            BuyerUsername = buyer?.Username ?? "Unknown",
            BuyerAvatar = buyer?.AvatarUrl ?? "",
            SellerId = sellerId,
            SellerUsername = seller?.Username ?? "Unknown",
            ProductId = review.ProductId ?? 0,
            ProductTitle = product?.Title ?? "Unknown Product",
            ProductImage = product?.Images ?? "",
            FeedbackType = root.GetProperty("feedbackType").GetString() ?? "neutral",
            Comment = root.GetProperty("comment").GetString() ?? "",
            ItemDescriptionRating = ratings.GetProperty("itemDescription").GetInt32(),
            CommunicationRating = ratings.GetProperty("communication").GetInt32(),
            ShippingSpeedRating = ratings.GetProperty("shippingSpeed").GetInt32(),
            ShippingCostRating = ratings.GetProperty("shippingCost").GetInt32(),
            IsVerifiedPurchase = root.TryGetProperty("isVerifiedPurchase", out var verified)
                ? verified.GetBoolean()
                : true,
            CreatedAt = review.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = root.TryGetProperty("updatedAt", out var updated)
                ? DateTime.Parse(updated.GetString() ?? "")
                : null,
            IsEdited = root.TryGetProperty("isEdited", out var edited)
                ? edited.GetBoolean()
                : false
        };
    }

    private JsonDocument ParseFeedbackData(string? json)
    {
        if (string.IsNullOrEmpty(json))
            throw new Exception("Invalid feedback data");

        return JsonDocument.Parse(json);
    }

    private bool IsFeedbackReview(Review review)
    {
        return review.Comment != null && review.Comment.Contains("\"feedbackType\"");
    }

    private FeedbackPeriodStatsDto CalculatePeriodStats(List<FeedbackDto> feedbacks, DateTime since)
    {
        var periodFeedbacks = feedbacks.Where(f => f.CreatedAt >= since).ToList();
        var total = periodFeedbacks.Count;
        var positive = periodFeedbacks.Count(f => f.FeedbackType == "positive");
        var neutral = periodFeedbacks.Count(f => f.FeedbackType == "neutral");
        var negative = periodFeedbacks.Count(f => f.FeedbackType == "negative");

        // FIX: Explicit decimal conversion
        var rate = total > 0 ? (decimal)positive / total * 100m : 0m;

        return new FeedbackPeriodStatsDto
        {
            TotalFeedbacks = total,
            PositiveCount = positive,
            NeutralCount = neutral,
            NegativeCount = negative,
            PositiveRate = Math.Round(rate, 1)
        };
    }
}