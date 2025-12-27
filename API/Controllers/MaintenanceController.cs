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

        // 2. Tạo mới: POST /api/maintenance
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

        // 3. Lấy danh sách (Lọc/Tìm kiếm): GET /api/maintenance
        // URL mẫu: /api/maintenances?studentId=123&status=pending&keyword=broken
        [HttpGet]
        public async Task<IActionResult> GetMaintenances(
            [FromQuery] string? studentId,
            [FromQuery] string? keyword,
            [FromQuery] string? status,
            [FromQuery] string? equipmentName)
        {
            bool success ;
            string message;
            int statusCode;
            IEnumerable<SummaryMaintenanceDto> data;
            if (!string.IsNullOrEmpty(studentId))
            {
                (success, message, statusCode, data) = await _maintenanceService.GetRequestsByStudentIdAsync(studentId);
            }
            else
            {
                (success, message, statusCode, data) = await _maintenanceService.GetMaintenanceFiltered(keyword, status, equipmentName);
            }
   
            if (!success)
            {
                return StatusCode(statusCode, new { message = message });
            }

            return Ok(new
            {
                message = message,
                data = data // Hoặc result.list tùy vào response của service
            });
        }

        // 4. Lấy chi tiết: GET /api/maintenance/{id}
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

        // 5. Cập nhật trạng thái: PATCH /api/maintenance/{id}/status
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

        // 6. Overview: GET /api/maintenance/overview
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

        // 7. Lấy mã hóa đơn: GET /api/maintenance/{id}
        [HttpGet("{requestId}/receipt")] 
        public async Task<IActionResult> GetReceiptIdByRequestId([FromRoute] string requestId)
        {
            // Lưu ý: Đổi tên tham số từ 'id' thành 'requestId' cho rõ ràng
            var result = await _maintenanceService.GetReceiptPendingMaintenance(requestId);

            return StatusCode(result.StatusCode, new
            {
                message = result.Message,
                data = result.receiptId
            });
        }
    }
}