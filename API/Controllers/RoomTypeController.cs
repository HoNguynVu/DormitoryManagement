using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomTypeController : Controller
    {
        private readonly IRoomTypeService _roomTypeService;
        public RoomTypeController(IRoomTypeService roomTypeService)
        {
            _roomTypeService = roomTypeService;
        }
        [HttpGet("registration")]
        public async Task<IActionResult> GetAllRoomTypes()
        {
            var (success, message, statusCode, roomTypes) = await _roomTypeService.GetAllRoomTypesAsync();
            if (success)
            {
                return StatusCode(statusCode, new
                {
                    success = true,
                    message = message,
                    data = roomTypes
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
