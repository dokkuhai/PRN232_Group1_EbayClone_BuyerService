using EbayCloneBuyerService_CoreAPI.DTOs.Notification;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using EbayCloneBuyerService_CoreAPI.Services.Interface;
using Microsoft.AspNetCore.SignalR;

namespace EbayCloneBuyerService_CoreAPI.Services.Impl
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            INotificationRepository notificationRepo,
            IHubContext<NotificationHub> hubContext)
        {
            _notificationRepo = notificationRepo;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, bool unreadOnly = false)
        {
            return await _notificationRepo.GetByUserIdAsync(userId, unreadOnly);
        }

        public async Task<NotificationDto> GetByIdAsync(int notificationId, int userId)
        {
            var notification = await _notificationRepo.GetByIdAsync(notificationId);

            // Verify ownership
            if (notification == null || notification.UserId != userId)
            {
                return null;
            }

            return notification;
        }

        public async Task<NotificationDto> CreateAsync(CreateNotificationRequest request)
        {
            var notification = await _notificationRepo.CreateAsync(request);

            // === PUSH NOTIFICATION qua SignalR ===
            await PushNotificationAsync(notification);

            return notification;
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            return await _notificationRepo.MarkAsReadAsync(notificationId, userId);
        }

        public async Task<int> MarkAllAsReadAsync(int userId)
        {
            var count = await _notificationRepo.MarkAllAsReadAsync(userId);

            // Notify client để update UI
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("AllNotificationsRead");

            return count;
        }

        public async Task<bool> DeleteAsync(int notificationId, int userId)
        {
            return await _notificationRepo.DeleteAsync(notificationId, userId);
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _notificationRepo.GetUnreadCountAsync(userId);
        }

        public async Task<int> BroadcastAsync(IEnumerable<int> userIds, string type, string title, string message)
        {
            var request = new CreateNotificationRequest
            {
                UserId = 0, // Will be overridden
                Type = type,
                Title = title,
                Message = message
            };

            var count = await _notificationRepo.BroadcastAsync(userIds, request);

            // Push notification qua SignalR cho tất cả users
            foreach (var userId in userIds)
            {
                try
                {
                    var notification = await _notificationRepo.GetByIdAsync(userId);
                    if (notification != null)
                    {
                        await PushNotificationAsync(notification);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to push notification to user {userId}: {ex.Message}");
                }
            }

            return count;
        }

        // === HELPER METHODS ===

        public async Task NotifyNewReviewAsync(int sellerId, int productId, string productTitle, int rating, string reviewerName)
        {
            var request = new CreateNotificationRequest
            {
                UserId = sellerId,
                Type = "REVIEW",
                Title = "New Review on Your Product",
                Message = $"{reviewerName} left a {rating}-star review on {productTitle}",
                ReferenceId = productId,
                ReferenceType = "PRODUCT",
                Metadata = new Dictionary<string, object>
                {
                    { "rating", rating },
                    { "reviewerName", reviewerName }
                }
            };

            await CreateAsync(request);
        }

        public async Task NotifyOrderUpdateAsync(int buyerId, int orderId, string status, string message)
        {
            var request = new CreateNotificationRequest
            {
                UserId = buyerId,
                Type = "ORDER_UPDATE",
                Title = $"Order Status: {status}",
                Message = message,
                ReferenceId = orderId,
                ReferenceType = "ORDER"
            };

            await CreateAsync(request);
        }

        public async Task NotifyPromotionAsync(int userId, string title, string message, int? productId = null)
        {
            var request = new CreateNotificationRequest
            {
                UserId = userId,
                Type = "PROMOTION",
                Title = title,
                Message = message,
                ReferenceId = productId,
                ReferenceType = productId.HasValue ? "PRODUCT" : null
            };

            await CreateAsync(request);
        }

        public async Task NotifyAdminFeedbackAsync(int userId, string title, string message)
        {
            var request = new CreateNotificationRequest
            {
                UserId = userId,
                Type = "ADMIN_FEEDBACK",
                Title = title,
                Message = message
            };

            await CreateAsync(request);
        }

        /// <summary>
        /// Helper: Push notification qua SignalR Hub
        /// </summary>
        private async Task PushNotificationAsync(NotificationDto notification)
        {
            try
            {
                await _hubContext.Clients.User(notification.UserId.ToString())
                    .SendAsync("ReceiveNotification", notification);

                // Cập nhật unread count
                var unreadCount = await GetUnreadCountAsync(notification.UserId);
                await _hubContext.Clients.User(notification.UserId.ToString())
                    .SendAsync("UpdateUnreadCount", unreadCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to push notification via SignalR: {ex.Message}");
            }
        }
    }
}




