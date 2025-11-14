using EbayCloneBuyerService_CoreAPI.DTOs.Feedback;

namespace EbayCloneBuyerService_CoreAPI.Services.Interface
{
    public interface IFeedbackService
    {
        Task<FeedbackDto> LeaveFeedbackAsync(int buyerId, CreateFeedbackDto dto);
        Task<FeedbackStatsDto> GetSellerFeedbackStatsAsync(int sellerId);
        Task<List<FeedbackDto>> GetSellerFeedbacksAsync(int sellerId, int page = 1, int pageSize = 20);
        Task<List<FeedbackDto>> GetBuyerFeedbacksAsync(int buyerId);
        Task<List<FeedbackDto>> GetReceivedFeedbacksAsync(int sellerId, int page = 1, int pageSize = 20);
        Task<CanLeaveFeedbackDto> CanLeaveFeedbackAsync(int buyerId, int orderId);
        Task<FeedbackDto?> GetFeedbackByIdAsync(int feedbackId);
        Task<FeedbackDto> UpdateFeedbackAsync(int buyerId, int feedbackId, UpdateFeedbackDto dto);
        Task<bool> DeleteFeedbackAsync(int buyerId, int feedbackId);
        Task UpdateSellerFeedbackSummaryAsync(int sellerId);
    }
}
