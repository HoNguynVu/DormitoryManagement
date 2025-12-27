using BusinessObject.Entities;

namespace API.Services.Interfaces
{
    public interface INotificationService
    {
        Task<(bool Success, string Message, int StatusCode, IEnumerable<Notification>)> GetLastestnotificationOfAccountId(string accountId);
        Task<(bool Success, string Message, int StatusCode)> MarkNotificationsAsReadAsync(string notiId);
    }
}
