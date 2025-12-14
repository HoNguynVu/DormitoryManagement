using BusinessObject.Entities;

namespace API.Services.Helpers
{
    public static class NotificationServiceHelpers
    {
        public static Notification CreateNew(string accountId, string title, string message, string type)
        {
            return new Notification
            {
                NotificationID = "NTF-" + IdGenerator.GenerateUniqueSuffix(),
                AccountID = accountId,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
