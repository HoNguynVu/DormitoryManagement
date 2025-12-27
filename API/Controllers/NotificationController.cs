using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        [HttpGet("latest/{accountId}")]
        public async Task<IActionResult> GetLatestNotifications(string accountId)
        {
            var (success, message, statusCode, notifications) = await _notificationService.GetLastestnotificationOfAccountId(accountId);
            if (success)
            {
                return StatusCode(statusCode, new { message, notifications });
            }
            else
            {
                return StatusCode(statusCode, new { message });
            }
        }

        [HttpPut("mark-as-read/{notiId}")]
        public async Task<IActionResult> MarkNotificationAsRead(string notiId)
        {
            var (success, message, statusCode) = await _notificationService.MarkNotificationsAsReadAsync(notiId);
            if (success)
            {
                return StatusCode(statusCode, new { message });
            }
            else
            {
                return StatusCode(statusCode, new { message });
            }
        }
    }
}
