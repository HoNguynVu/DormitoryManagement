using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoomsForRegistration()
        {
            var (success, message, statusCode, rooms) = await _roomService.GetRoomForRegistration();

            if (success)
            {
                return StatusCode(statusCode, new
                {
                    success = true,
                    message = message,
                    data = rooms
                });
            }

            return StatusCode(statusCode, new
            {
                success = false,
                message = message
            });
        }
    }
}
