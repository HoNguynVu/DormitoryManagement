using API.Services.Interfaces;
using BusinessObject.DTOs.HealthInsuranceDTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthInsuranceController : ControllerBase
    {
        private readonly IHealthInsuranceService _healthInsuranceService;

        public HealthInsuranceController(IHealthInsuranceService healthInsuranceService)
        {
            _healthInsuranceService = healthInsuranceService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterInsurance([FromBody] HealthInsuranceRequestDto request)
        {

            if (request == null) 
                return BadRequest(new { message = "Invalid request data." });

            var result = await _healthInsuranceService.RegisterHealthInsuranceAsync(request.StudentId, request.HospitalId,request.CardNumber);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            // Trả về thành công 201 kèm thông báo và ID bảo hiểm
            return StatusCode(result.StatusCode, new
            {
                message = "Insurance application created. Please proceed to payment.",
                insuranceId = result.Message
            });
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentInsurance(string studentId)
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

        [HttpPut("{insuranceId}/confirm")]
        public async Task<IActionResult> ConfirmPayment(string insuranceId)
        {
            // 1. Gọi Service
            var result = await _healthInsuranceService.ConfirmInsurancePaymentAsync(insuranceId);

            // 2. Xử lý kết quả trả về
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            // 3. Thành công (200 OK)
            return Ok(new { message = result.Message });
        }

        [HttpPost("add-price")]
        public async Task<IActionResult> CreateHealthPrice([FromBody] CreateHealthPriceDTO dto)
        {
            var result = await _healthInsuranceService.CreateHealthInsurancePriceAsync(dto);
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { Message = result.Message });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }
    }
}
