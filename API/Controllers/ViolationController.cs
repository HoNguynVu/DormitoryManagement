using API.Services.Interfaces;
using BusinessObject.DTOs.ViolationDTOs;
using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ViolationController : ControllerBase
    {
        private readonly IViolationService _violationService;

        public ViolationController(IViolationService violationService)
        {
            _violationService = violationService;
        }

        /// <summary>
        /// Trưởng tòa tạo vi phạm mới (Resolution để trống)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateViolation([FromBody] CreateViolationRequest request)
        {
            var result = await _violationService.CreateViolationAsync(request);
            return StatusCode(result.StatusCode, new
            {
                success = result.Success,
                message = result.Message,
                data = result.Data
            });
        }

        /// <summary>
        /// Cập nhật hướng xử lý vi phạm (sau khi gặp sinh viên hoặc tự quyết định)
        /// </summary>
        [HttpPut("resolution")]
        public async Task<IActionResult> UpdateResolution([FromBody] UpdateViolationRequest request)
        {
            var result = await _violationService.UpdateViolationAsync(request);
            return StatusCode(result.StatusCode, new
            {
                success = result.Success,
                message = result.Message
            });
        }

        /// <summary>
        /// Lấy danh sách vi phạm của 1 sinh viên
        /// </summary>
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetViolationsByStudentId(string studentId)
        {
            var result = await _violationService.GetViolationsByStudentIdAsync(studentId);
            return StatusCode(result.StatusCode, new
            {
                success = result.Success,
                message = result.Message,
                data = result.Data
            });
        }

        /// <summary>
        /// Lấy tất cả vi phạm
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllViolations()
        {
            var result = await _violationService.GetAllViolationsAsync();
            return StatusCode(result.StatusCode, new
            {
                success = result.Success,
                message = result.Message,
                data = result.Data
            });
        }

        /// <summary>
        /// Lấy danh sách vi phạm chưa xử lý (Resolution null hoặc rỗng)
        /// </summary>
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingViolations()
        {
            var result = await _violationService.GetPendingViolationsAsync();
            return StatusCode(result.StatusCode, new
            {
                success = result.Success,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpGet("manager/dashboard/{accountId}")]
        public async Task<IActionResult> GetViolationDashboard(string accountId)
        {
            var result = await _violationService.GetViolationStatsByManagerAsync(accountId);
            return StatusCode(result.StatusCode, new
            {
                success = result.Success,
                message = result.Message,
                data = result.Data
            });
        }
    }
}
