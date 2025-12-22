using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentController : ControllerBase
    {
        private readonly IEquipmentService _equipmentService;
        public EquipmentController(IEquipmentService equipmentService)
        {
            _equipmentService = equipmentService;
        }
        [HttpGet("equipments/{roomId}")]
        public async Task<IActionResult> GetEquipmentByRoomId(string roomId)
        {
            var result = await _equipmentService.GetAllEquipmentByRoomIdAsync(roomId);
            if (result.Success)
            {
                return Ok(new
                {
                    Message = result.Message,
                    Data = result.result
                });
            }
            else
            {
                return StatusCode(result.StatusCode, new
                {
                    Message = result.Message
                });
            }
        }
    }
}
