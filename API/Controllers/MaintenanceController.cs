using API.Services.Interfaces;
using BusinessObject.DTOs.MaintenanceDTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/maintenances")]
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceService _maintenanceService;
        public MaintenanceController(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        // 2. Tạo mới: POST /api/maintenances
        [HttpPost]
        public async Task<IActionResult> CreateRequest([FromBody] CreateMaintenanceDto dto)
        {
            if (dto == null) return BadRequest("Invalid request data.");

            var result = await _maintenanceService.CreateRequestAsync(dto);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            return StatusCode(201, new { message = result.Message, data = result.requestMaintenanceId });
        }

        // 3. Lấy danh sách (Lọc/Tìm kiếm): GET /api/maintenances
        // URL mẫu: /api/maintenances?studentId=123&status=pending&keyword=broken
        [HttpGet]
        public async Task<IActionResult> GetMaintenances(
            [FromQuery] string? studentId,
            [FromQuery] string? keyword,
            [FromQuery] string? status,
            [FromQuery] string? equipmentName)
        {
            // Logic xử lý: Nếu có studentId thì gọi service get by student, 
            // nếu có keyword/status thì gọi filter. 
            // Tốt nhất nên gộp Logic này vào 1 hàm trong Service nhận vào 1 object filter.
            dynamic result;
            if (!string.IsNullOrEmpty(studentId))
            {
                result = await _maintenanceService.GetRequestsByStudentIdAsync(studentId);
            }
            else
            {
                result = await _maintenanceService.GetMaintenanceFiltered(keyword, status, equipmentName);
            }

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            return Ok(new
            {
                message = result.Message,
                data = result.dto // Hoặc result.list tùy vào response của service
            });
        }

        // 4. Lấy chi tiết: GET /api/maintenances/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMaintenanceDetail([FromRoute] string id)
        {
            var result = await _maintenanceService.GetMaintenanceDetail(id);
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return Ok(new
            {
                message = result.Message,
                data = result.dto
            });
        }

        // 5. Cập nhật trạng thái: PATCH /api/maintenances/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus([FromRoute] string id, [FromBody] UpdateMaintenanceStatusDto dto)
        {
            if (dto == null) return BadRequest("Invalid request data.");

            // Gán ID từ route vào DTO (nếu DTO cần ID để xử lý) hoặc truyền ID vào service
            // dto.Id = id; 

            var result = await _maintenanceService.UpdateStatusAsync(dto); // Cần đảm bảo Service biết update cho ID nào

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        // 6. Overview: GET /api/maintenances/overview
        [HttpGet("overview")]
        public async Task<IActionResult> GetOverviewMaintenance()
        {
            var result = await _maintenanceService.GetOverviewMaintenance();
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return Ok(new
            {
                message = result.Message,
                data = result.list
            });
        }
    }
}