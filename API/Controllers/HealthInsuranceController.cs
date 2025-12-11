using API.Services.Interfaces;
using BusinessObject.DTOs.HealthInsuranceDTOs;
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

            var result = await _healthInsuranceService.RegisterHealthInsuranceAsync(request.StudentId, request.InitialHospital);

            if (!result.Success)
            {
                // Trả về lỗi 400, 404, 409 hoặc 500 kèm thông báo từ Service
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
    }
}
