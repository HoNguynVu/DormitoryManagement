using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            // SignalR sẽ tìm giá trị của claim "accountId" để làm UserID
            return connection.User?.FindFirst("accountId")?.Value;
        }
    }
}
