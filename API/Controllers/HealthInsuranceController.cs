using API.Services.Implements;
using API.Services.Interfaces;
using BusinessObject.DTOs.HealthInsuranceDTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/health-insurances")]
    [ApiController]
    public class HealthInsuranceController : ControllerBase
    {
        private readonly IHealthInsuranceService _healthInsuranceService;

        public HealthInsuranceController(IHealthInsuranceService healthInsuranceService)
        {
            _healthInsuranceService = healthInsuranceService;
        }

        // ================= GET METHODS (READ) =================

        // GET: api/health-insurances
        // Thay thế cho /filtered. Dùng Query Params để lọc.
        [HttpGet]
        public async Task<IActionResult> GetHealthInsurances(
            [FromQuery] string? keyword,
            [FromQuery] string? hospitalName,
            [FromQuery] int? year,
            [FromQuery] string? status)
        {
            var result = await _healthInsuranceService.GetHealthInsuranceFiltered(keyword, hospitalName, year, status);

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

        // GET: api/health-insurances/{id}
        // Thay thế cho /detail/{insuranceId}. ID nằm trên Route.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetailHealthInsurance([FromRoute] string id)
        {
            // Code cũ của bạn dùng [FromQuery] trong khi route có param -> Sai logic
            // Đã sửa thành [FromRoute]
            var result = await _healthInsuranceService.GetDetailHealth(id);

            if (!result.Success)
                return StatusCode(result.StatusCode, new { Message = result.Message });

            return Ok(new
            {
                message = result.Message,
                data = result.dto // Giả sử result trả về dto ở đây
            });
        }

        // GET: api/health-insurances/prices
        [HttpGet("prices")]
        public async Task<IActionResult> GetPriceHealthInsurance([FromQuery] int year)
        {

            var result = await _healthInsuranceService.GetHealthPriceByYear(year);
            if (!result.Success)
                return StatusCode(result.StatusCode, new { Message = result.Message });

            return Ok(new
            {
                message = result.Message,
                data = result.dto 
            });
        }

        // GET: api/health-insurances/students/{studentId}
        // Lấy BHYT theo Student ID (Mapping resource quan hệ)
        [HttpGet("students/{studentId}")]
        public async Task<IActionResult> GetStudentInsurance([FromRoute] string studentId)
        {
            var result = await _healthInsuranceService.GetInsuranceByStudentIdAsync(studentId);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return Ok(new
            {
                message = result.Message,
                data = result.Data
            });
        }

        // ================= POST/PUT/PATCH METHODS (WRITE) =================

        // POST: api/health-insurances
        // Thay thế cho /register. POST vào root collection nghĩa là tạo mới.
        [HttpPost]
        public async Task<IActionResult> RegisterInsurance([FromBody] HealthInsuranceRequestDto request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request data." });

            var result = await _healthInsuranceService.RegisterHealthInsuranceAsync(request.StudentId, request.HospitalId, request.CardNumber);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            // Trả về 201 Created
            return StatusCode(result.StatusCode, new
            {
                message = "Insurance application created. Please proceed to payment.",
                insuranceId = result.insuranceId
            });
        }

        // POST: api/health-insurances/{id}/payment-confirmations
        // Thay thế cho /confirm. Đây là một hành động xác nhận thanh toán.
        // Có thể dùng POST (tạo confirmation) hoặc PATCH (sửa status).
        // Ở đây dùng POST để thể hiện hành động "Confirm".
        [HttpPost("{id}/payment-confirmations")]
        public async Task<IActionResult> ConfirmPayment([FromRoute] string id)
        {
            // Lưu ý: ID lấy từ Route
            var result = await _healthInsuranceService.ConfirmInsurancePaymentAsync(id);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        // POST: api/health-insurances/prices
        // Thay thế cho /add-price. "Price" là một sub-resource hoặc cấu hình.
        [HttpPost("prices")]
        public async Task<IActionResult> CreateHealthPrice([FromBody] CreateHealthPriceDTO dto)
        {
            var result = await _healthInsuranceService.CreateHealthInsurancePriceAsync(dto);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { Message = result.Message });
            }

            // Trả về 201 Created nếu tạo mới thành công
            return StatusCode(result.StatusCode, new { message = result.Message ,priceId=result.healthPriceId});
        }
    }
}