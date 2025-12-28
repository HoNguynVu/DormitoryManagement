using API.Services.Interfaces;
using BusinessObject.DTOs.RoomTypeDTOs;
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

        [HttpPut]
        public async Task<IActionResult> UpdateRoomType([FromBody] UpdateRoomTypeDTO updateRoomTypeDTO)
        {
            var (success, message, statusCode) = await _roomTypeService.UpdateRoomTypeAsync(updateRoomTypeDTO);
            if (success)
            {
                return StatusCode(statusCode, new
                {
                    success = true,
                    message = message
                });
            }
            return StatusCode(statusCode, new
            {
                success = false,
                message = message
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoomType([FromBody] CreateRoomTypeDTO createRoomTypeDTO)
        {
            var (success, message, statusCode) = await _roomTypeService.CreateRoomTypeAsync(createRoomTypeDTO);
            if (success)
            {
                return StatusCode(statusCode, new
                {
                    success = true,
                    message = message
                });
            }
            return StatusCode(statusCode, new
            {
                success = false,
                message = message
            });
        }

        [HttpDelete("{typeId}")]
        public async Task<IActionResult> DeleteRoomType([FromRoute] string typeId)
        {
            var (success, message, statusCode) = await _roomTypeService.DeleteRoomTypeAsync(typeId);
            if (success)
            {
                return StatusCode(statusCode, new
                {
                    success = true,
                    message = message
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
