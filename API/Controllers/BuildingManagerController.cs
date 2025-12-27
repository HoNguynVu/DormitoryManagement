using API.Services.Interfaces;
using BusinessObject.DTOs.BuildingManagerDTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BuildingManagerController : ControllerBase
    {
        private readonly IBuildingManagerService _service;

        public BuildingManagerController(IBuildingManagerService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllManagersAsync();
            return Ok(new { success = true, data = list });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { success = false, message = "Manager id is required" });
            var m = await _service.GetManagerByIdAsync(id);
            if (m == null) return NotFound(new { success = false, message = "Manager not found" });
            return Ok(new { success = true, data = m });
        }

        [HttpGet("dashboard-stats/{accountId}")]
        public async Task<IActionResult> GetDashboardStats(string accountId)
        {
            var (success, message, statusCode, data) = await _service.GetDashboardStatsAsync(accountId);
            if (success)
            {
                return StatusCode(statusCode, new { success = true, message = message, data = data });
            }
            return StatusCode(statusCode, new { success = false, message = message });
        }

        [HttpPost("receipts")]
        public async Task<IActionResult> GetReceipts([FromBody] GetReceiptRequest request)
        {
            var (success, message, statusCode, data) = await _service.GetReceiptsAsync(request);
            if (success)
            {
                return StatusCode(statusCode, new { success = true, message = message, data = data });
            }
            return StatusCode(statusCode, new { success = false, message = message });
        }
    } 
}
