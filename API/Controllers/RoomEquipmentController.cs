using API.Services.Interfaces;
using BusinessObject.DTOs.EquipmentDTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomEquipmentController : ControllerBase
    {
        private readonly IRoomEquipmentService _roomEquipmentService;
        public RoomEquipmentController(IRoomEquipmentService roomEquipmentService)
        {
            _roomEquipmentService = roomEquipmentService;
        }

        [HttpPost("change-status")]
        public async Task<IActionResult> ChangeStatus([FromBody] ChangeStatusRoomEquipmentDto dto)
        {
            var result = await _roomEquipmentService.ChangeStatusAsync(
                dto.RoomId,
                dto.EquipmentId,
                dto.Quantity,
                dto.FromStatus,
                dto.ToStatus
            );
            if (result.Success)
            {
                return Ok(new { message = result.Message });
            }
            else
            {
                return StatusCode(result.StatusCode, new { error = result.Message });
            }
        }
    }
}
