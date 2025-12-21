using API.Services.Interfaces;
using BusinessObject.DTOs.MaintenanceDTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceService _maintenanceService;
        public MaintenanceController(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        [HttpPost("request")]
        public async Task<IActionResult> CreateRequest([FromBody] CreateMaintenanceDto dto)
        {
            if (dto == null) return BadRequest("Invalid request data.");

            var result = await _maintenanceService.CreateRequestAsync(dto);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            // Trả về 201 Created
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpGet("studentId/{studentId}")]
        public async Task<IActionResult> GetRequestByStudentId([FromQuery] string studentId)
        {
            var result = await _maintenanceService.GetRequestsByStudentIdAsync(studentId);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            // Trả về dữ liệu kèm message
            return Ok(new
            {
                message = result.Message,
                data = result.dto
            });
        }


        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateMaintenanceStatusDto dto)
        {
            if (dto == null) return BadRequest("Invalid request data.");

            var result = await _maintenanceService.UpdateStatusAsync(dto);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
    }

}
