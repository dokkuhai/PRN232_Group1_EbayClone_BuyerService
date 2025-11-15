using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace EbayCloneBuyerService_CoreAPI.Hubs
{
    public class NotificationHub : Hub
    {
        /// <summary>
        /// Được gọi khi client connect
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Add connection to user's group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                Console.WriteLine($"User {userId} connected to NotificationHub");
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Được gọi khi client disconnect
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
                Console.WriteLine($"User {userId} disconnected from NotificationHub");
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Client có thể gọi method này để request unread count
        /// </summary>
        public async Task RequestUnreadCount()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Service sẽ xử lý và gửi lại qua UpdateUnreadCount
                // Có thể inject INotificationService vào đây nếu cần
                await Clients.Caller.SendAsync("UpdateUnreadCount", 0); // Placeholder
            }
        }

        /// <summary>
        /// Client ping để keep connection alive
        /// </summary>
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
        }
    }
}
