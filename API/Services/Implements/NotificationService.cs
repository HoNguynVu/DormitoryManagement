using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.Entities;

namespace API.Services.Implements
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationUow _notificationUow;
        public NotificationService(INotificationUow notificationUow)
        {
            _notificationUow = notificationUow;
        }
        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<Notification>)> GetLastestnotificationOfAccountId(string accountId)
        {
            try
            {
                var notifications = await _notificationUow.Notifications.GetLastestNotificationsByAccountIdAsync(accountId);
                return (true, "Notifications retrieved successfully.", 200, notifications);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}", 500, Enumerable.Empty<BusinessObject.Entities.Notification>());
            }
        }
        public async Task<(bool Success, string Message, int StatusCode)> MarkNotificationsAsReadAsync(string notiId)
        {
            await _notificationUow.BeginTransactionAsync();
            try
            {
                var notification = await _notificationUow.Notifications.GetByIdAsync(notiId);
                if (notification == null)
                {
                    return (false, "Notification not found.", 404);
                }
                notification.IsRead = true;
                _notificationUow.Notifications.Update(notification);
                await _notificationUow.CommitAsync();
                return (true, "Notification marked as read.", 200);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}", 500);
            }
        }
    }
}
