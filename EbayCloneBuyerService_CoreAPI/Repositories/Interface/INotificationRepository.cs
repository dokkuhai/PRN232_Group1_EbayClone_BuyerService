using EbayCloneBuyerService_CoreAPI.DTOs.Notification;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Interface
{
    public interface INotificationRepository
    {
        /// <summary>
        /// Lấy tất cả notifications của user
        /// </summary>
        Task<IEnumerable<NotificationDto>> GetByUserIdAsync(int userId, bool unreadOnly = false, int limit = 50);

        /// <summary>
        /// Lấy notification theo ID
        /// </summary>
        Task<NotificationDto> GetByIdAsync(int id);

        /// <summary>
        /// Tạo notification mới
        /// </summary>
        Task<NotificationDto> CreateAsync(CreateNotificationRequest request);

        /// <summary>
        /// Đánh dấu notification đã đọc
        /// </summary>
        Task<bool> MarkAsReadAsync(int notificationId, int userId);

        /// <summary>
        /// Đánh dấu tất cả notifications của user là đã đọc
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
        /// Broadcast notification cho nhiều users
        /// </summary>
        Task<int> BroadcastAsync(IEnumerable<int> userIds, CreateNotificationRequest request);
    }
}