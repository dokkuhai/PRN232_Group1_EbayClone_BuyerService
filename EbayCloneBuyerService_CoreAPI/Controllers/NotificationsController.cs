using EbayCloneBuyerService_CoreAPI.DTOs.Notification;
using EbayCloneBuyerService_CoreAPI.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EbayCloneBuyerService_CoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// [GET] /api/notifications - Lấy tất cả notifications của user hiện tại (Pull)
        /// Query params: unreadOnly (bool)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<NotificationDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetAll([FromQuery] bool unreadOnly = false)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Authentication required" });
            }

            var notifications = await _notificationService.GetUserNotificationsAsync(userId.Value, unreadOnly);
            return Ok(notifications);
        }

        /// <summary>
        /// [GET] /api/notifications/{id} - Lấy 1 notification theo ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(NotificationDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Authentication required" });
            }

            var notification = await _notificationService.GetByIdAsync(id, userId.Value);
            if (notification == null)
            {
                return NotFound(new { message = "Notification not found" });
            }

            return Ok(notification);
        }

        /// <summary>
        /// [GET] /api/notifications/unread/count - Đếm số notification chưa đọc
        /// </summary>
        [HttpGet("unread/count")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Authentication required" });
            }

            var count = await _notificationService.GetUnreadCountAsync(userId.Value);
            return Ok(new { unreadCount = count });
        }

        /// <summary>
        /// [POST] /api/notifications/{id}/read - Đánh dấu notification đã đọc
        /// </summary>
        [HttpPost("{id}/read")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Authentication required" });
            }

            var success = await _notificationService.MarkAsReadAsync(id, userId.Value);
            if (!success)
            {
                return NotFound(new { message = "Notification not found" });
            }

            return Ok(new { message = "Notification marked as read" });
        }

        /// <summary>
        /// [POST] /api/notifications/read-all - Đánh dấu tất cả notifications là đã đọc
        /// </summary>
        [HttpPost("read-all")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Authentication required" });
            }

            var count = await _notificationService.MarkAllAsReadAsync(userId.Value);
            return Ok(new { message = $"{count} notifications marked as read" });
        }

        /// <summary>
        /// [DELETE] /api/notifications/{id} - Xóa notification
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Authentication required" });
            }

            var success = await _notificationService.DeleteAsync(id, userId.Value);
            if (!success)
            {
                return NotFound(new { message = "Notification not found" });
            }

            return NoContent();
        }

        /// <summary>
        /// [POST] /api/notifications - Tạo notification mới (ADMIN ONLY)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(NotificationDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Create([FromBody] CreateNotificationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // TODO: Check admin role from JWT
            if (!IsAdmin())
            {
                return Forbid("Admin access required");
            }

            var notification = await _notificationService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = notification.Id }, notification);
        }

        /// <summary>
        /// [POST] /api/notifications/broadcast - Broadcast notification cho nhiều users (ADMIN ONLY)
        /// Body: { userIds: [1,2,3], type: "PROMOTION", title: "...", message: "..." }
        /// </summary>
        [HttpPost("broadcast")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Broadcast([FromBody] BroadcastRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!IsAdmin())
            {
                return Forbid("Admin access required");
            }

            var count = await _notificationService.BroadcastAsync(
                request.UserIds,
                request.Type,
                request.Title,
                request.Message
            );

            return Ok(new { message = $"Notification sent to {count} users" });
        }

        // === HELPER METHODS ===

        private int? GetCurrentUserId()
        {
            // TODO: Implement JWT token parsing
            // TEMPORARY: Return mock userId for testing
            return 1;
        }

        private bool IsAdmin()
        {
            // TODO: Implement role checking from JWT
            // TEMPORARY: Return false for testing
            return false;
        }
    }

    /// <summary>
    /// Request DTO cho broadcast
    /// </summary>
    public class BroadcastRequest
    {
        public IEnumerable<int> UserIds { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
    }
}




