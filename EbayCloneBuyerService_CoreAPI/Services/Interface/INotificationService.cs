using EbayCloneBuyerService_CoreAPI.DTOs.Notification;

namespace EbayCloneBuyerService_CoreAPI.Services.Interface
{
    public interface INotificationService
    {
        /// <summary>
        /// Lấy tất cả notifications của user
        /// </summary>
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, bool unreadOnly = false);

        /// <summary>
        /// Lấy notification theo ID
        /// </summary>
        Task<NotificationDto> GetByIdAsync(int notificationId, int userId);

        /// <summary>
        /// Tạo notification mới (Admin/System)
        /// </summary>
        Task<NotificationDto> CreateAsync(CreateNotificationRequest request);

        /// <summary>
        /// Đánh dấu notification đã đọc
        /// </summary>
        Task<bool> MarkAsReadAsync(int notificationId, int userId);

        /// <summary>
        /// Đánh dấu tất cả notifications là đã đọc
        /// </summary>
        Task<int> MarkAllAsReadAsync(int userId);

        /// <summary>
        /// Xóa notification
        /// </summary>
        Task<bool> DeleteAsync(int notificationId, int userId);

        /// <summary>
        /// Đếm số notification chưa đọc
        /// </summary>
        Task<int> GetUnreadCountAsync(int userId);

        /// <summary>
        /// Broadcast notification cho nhiều users (Admin)
        /// </summary>
        Task<int> BroadcastAsync(IEnumerable<int> userIds, string type, string title, string message);

        // === Helper methods cho các module khác trigger notification ===

        /// <summary>
        /// Gửi thông báo khi có review mới (từ ReviewService)
        /// </summary>
        Task NotifyNewReviewAsync(int sellerId, int productId, string productTitle, int rating, string reviewerName);

        /// <summary>
        /// Gửi thông báo khi order status thay đổi (từ OrderService)
        /// </summary>
        Task NotifyOrderUpdateAsync(int buyerId, int orderId, string status, string message);

        /// <summary>
        /// Gửi thông báo promotion (từ Admin/Marketing)
        /// </summary>
        Task NotifyPromotionAsync(int userId, string title, string message, int? productId = null);

        /// <summary>
        /// Gửi thông báo từ Admin
        /// </summary>
        Task NotifyAdminFeedbackAsync(int userId, string title, string message);
    }
}





