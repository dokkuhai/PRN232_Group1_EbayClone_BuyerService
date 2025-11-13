using EbayCloneBuyerService_CoreAPI.DTOs.Notification;
using EbayCloneBuyerService_CoreAPI.Models;
using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EbayCloneBuyerService_CoreAPI.Repositories.Impl
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly CloneEbayDbContext _context;
        private const int SYSTEM_SENDER_ID = 0; // Notification từ hệ thống

        public NotificationRepository(CloneEbayDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NotificationDto>> GetByUserIdAsync(int userId, bool unreadOnly = false, int limit = 50)
        {
            var query = _context.Messages
                .Where(m => m.ReceiverId == userId && (m.SenderId == null || m.SenderId == SYSTEM_SENDER_ID))
                .OrderByDescending(m => m.Timestamp)
                .Take(limit);

            var messages = await query.ToListAsync();
            var notifications = new List<NotificationDto>();

            foreach (var message in messages)
            {
                var dto = ParseMessageToNotification(message);
                if (dto != null)
                {
                    if (!unreadOnly || !dto.IsRead)
                    {
                        notifications.Add(dto);
                    }
                }
            }

            return notifications;
        }

        public async Task<NotificationDto> GetByIdAsync(int id)
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == id && (m.SenderId == null || m.SenderId == SYSTEM_SENDER_ID));

            return message != null ? ParseMessageToNotification(message) : null;
        }

        public async Task<NotificationDto> CreateAsync(CreateNotificationRequest request)
        {
            var notificationContent = new NotificationContent
            {
                Type = request.Type,
                Title = request.Title,
                Message = request.Message,
                ReferenceId = request.ReferenceId,
                ReferenceType = request.ReferenceType,
                IsRead = false,
                Metadata = request.Metadata ?? new Dictionary<string, object>()
            };

            var message = new Message
            {
                SenderId = SYSTEM_SENDER_ID,
                ReceiverId = request.UserId,
                Content = JsonSerializer.Serialize(notificationContent),
                Timestamp = DateTime.Now
            };

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            return ParseMessageToNotification(message);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == notificationId
                                      && m.ReceiverId == userId
                                      && (m.SenderId == null || m.SenderId == SYSTEM_SENDER_ID));

            if (message == null) return false;

            try
            {
                var content = JsonSerializer.Deserialize<NotificationContent>(message.Content);
                if (content != null)
                {
                    content.IsRead = true;
                    message.Content = JsonSerializer.Serialize(content);
                    _context.Messages.Update(message);
                    await _context.SaveChangesAsync();
                    return true;
                }
            }
            catch (JsonException)
            {
                return false;
            }

            return false;
        }

        public async Task<int> MarkAllAsReadAsync(int userId)
        {
            var messages = await _context.Messages
                .Where(m => m.ReceiverId == userId && (m.SenderId == null || m.SenderId == SYSTEM_SENDER_ID))
                .ToListAsync();

            int count = 0;
            foreach (var message in messages)
            {
                try
                {
                    var content = JsonSerializer.Deserialize<NotificationContent>(message.Content);
                    if (content != null && !content.IsRead)
                    {
                        content.IsRead = true;
                        message.Content = JsonSerializer.Serialize(content);
                        count++;
                    }
                }
                catch (JsonException)
                {
                    continue;
                }
            }

            if (count > 0)
            {
                await _context.SaveChangesAsync();
            }

            return count;
        }

        public async Task<bool> DeleteAsync(int notificationId, int userId)
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == notificationId
                                      && m.ReceiverId == userId
                                      && (m.SenderId == null || m.SenderId == SYSTEM_SENDER_ID));

            if (message == null) return false;

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            var messages = await _context.Messages
                .Where(m => m.ReceiverId == userId && (m.SenderId == null || m.SenderId == SYSTEM_SENDER_ID))
                .ToListAsync();

            int count = 0;
            foreach (var message in messages)
            {
                try
                {
                    var content = JsonSerializer.Deserialize<NotificationContent>(message.Content);
                    if (content != null && !content.IsRead)
                    {
                        count++;
                    }
                }
                catch (JsonException)
                {
                    continue;
                }
            }

            return count;
        }

        public async Task<int> BroadcastAsync(IEnumerable<int> userIds, CreateNotificationRequest request)
        {
            var notificationContent = new NotificationContent
            {
                Type = request.Type,
                Title = request.Title,
                Message = request.Message,
                ReferenceId = request.ReferenceId,
                ReferenceType = request.ReferenceType,
                IsRead = false,
                Metadata = request.Metadata ?? new Dictionary<string, object>()
            };

            var contentJson = JsonSerializer.Serialize(notificationContent);
            var messages = userIds.Select(userId => new Message
            {
                SenderId = SYSTEM_SENDER_ID,
                ReceiverId = userId,
                Content = contentJson,
                Timestamp = DateTime.Now
            }).ToList();

            await _context.Messages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();

            return messages.Count;
        }

        /// <summary>
        /// Helper: Parse Message entity thành NotificationDto
        /// </summary>
        private NotificationDto ParseMessageToNotification(Message message)
        {
            try
            {
                var content = JsonSerializer.Deserialize<NotificationContent>(message.Content);
                if (content == null) return null;

                return new NotificationDto
                {
                    Id = message.Id,
                    UserId = message.ReceiverId ?? 0,
                    Type = content.Type,
                    Title = content.Title,
                    Message = content.Message,
                    ReferenceId = content.ReferenceId,
                    ReferenceType = content.ReferenceType,
                    IsRead = content.IsRead,
                    CreatedAt = message.Timestamp ?? DateTime.Now,
                    Metadata = content.Metadata
                };
            }
            catch (JsonException)
            {
                // Fallback: Nếu content không phải JSON, coi như plain message
                return new NotificationDto
                {
                    Id = message.Id,
                    UserId = message.ReceiverId ?? 0,
                    Type = "SYSTEM",
                    Title = "System Message",
                    Message = message.Content,
                    IsRead = false,
                    CreatedAt = message.Timestamp ?? DateTime.Now,
                    Metadata = new Dictionary<string, object>()
                };
            }
        }
    }
}