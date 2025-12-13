using API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessObject.DTOs.RoomDTOs;

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

        // Admin endpoints
        [HttpPost]
        //[Authorize("Admin")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
        {
            var (success, message, statusCode, room) = await _roomService.CreateRoomAsync(request);
            if (success)
                return StatusCode(statusCode, new { success = true, message, data = room });

            return StatusCode(statusCode, new { success = false, message });
        }

        [HttpPut]
        //[Authorize("Admin")]
        public async Task<IActionResult> UpdateRoom([FromBody] UpdateRoomDto request)
        {
            var (success, message, statusCode) = await _roomService.UpdateRoomAsync(request);
            if (success) return StatusCode(statusCode, new { success = true, message });

            return StatusCode(statusCode, new { success = false, message });
        }

        [HttpDelete("{roomId}")]
        //[Authorize("Admin")]
        public async Task<IActionResult> DeleteRoom(string roomId)
        {
            var (success, message, statusCode) = await _roomService.DeleteRoomAsync(roomId);
            if (success) return StatusCode(statusCode, new { success = true, message });

            return StatusCode(statusCode, new { success = false, message });
        }

        [HttpGet("{roomId}/status")]
        //[Authorize("Admin")]
        public async Task<IActionResult> GetRoomStatus(string roomId)
        {
            var (success, message, statusCode, roomStatus) = await _roomService.GetRoomStatusAsync(roomId);
            if (success)
                return StatusCode(statusCode, new { success = true, message, data = roomStatus });
            return StatusCode(statusCode, new { success = false, message });
        }

        [HttpPost("available")]
        //[Authorize("Admin")]
        public async Task<IActionResult> GetAvailableRooms([FromBody] RoomFilterDto filter)
        {
            var (success, message, statusCode, availableRooms) = await _roomService.GetAvailableRoomsAsync(filter);
            if (success)
                return StatusCode(statusCode, new { success = true, message, data = availableRooms });
            return StatusCode(statusCode, new { success = false, message });
        }
    }
}
