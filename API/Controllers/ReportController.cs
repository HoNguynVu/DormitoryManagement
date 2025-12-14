using API.Services.Interfaces;
using BusinessObject.DTOs.ReportDTOs;
using BusinessObject.DTOs.RoomDTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("priority")]
        public async Task<IActionResult> GetStudentsByPriority([FromQuery] string? priorityId)
        {
            var students = await _reportService.GetStudentsByPriorityAsync(priorityId);
            return Ok(new { success = true, data = students });
        }

        [HttpGet("expired-contracts")]
        public async Task<IActionResult> GetExpiredContracts([FromQuery] string? beforeDate)
        {
            DateOnly cutoff;
            if (string.IsNullOrWhiteSpace(beforeDate)) cutoff = DateOnly.FromDateTime(DateTime.UtcNow);
            else if (!DateOnly.TryParse(beforeDate, out cutoff)) return BadRequest(new { success = false, message = "Invalid date format (YYYY-MM-DD)" });

            var list = await _reportService.GetExpiredContractsAsync(cutoff);
            return Ok(new { success = true, data = list });
        }

        [HttpGet("student/{studentId}/contracts")]
        public async Task<IActionResult> GetContractsByStudent(string studentId)
        {
            var list = await _reportService.GetContractsByStudentAsync(studentId);
            return Ok(new { success = true, data = list });
        }

        // New endpoint: equipment status for a room
        [HttpGet("room/{roomId}/equipment")]
        public async Task<IActionResult> GetEquipmentByRoom(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId)) return BadRequest(new { success = false, message = "RoomId is required" });

            var items = await _reportService.GetEquipmentStatusByRoomAsync(roomId);
            return Ok(new { success = true, data = items });
        }
    }
}
