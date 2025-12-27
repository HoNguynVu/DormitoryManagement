using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR; 

namespace API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}
