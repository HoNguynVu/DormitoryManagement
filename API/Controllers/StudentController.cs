using API.Services.Interfaces;
using BusinessObject.DTOs.StudentDTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }
        [HttpGet]
        public async Task<IActionResult> GetStudentByID(string accountId)
        {
            var result = await _studentService.GetStudentByID(accountId);
            if (result.Success)
            {
                return StatusCode(result.StatusCode, new
                {
                    message = result.Message,
                    student = result.student
                });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPost("update-info")]
        public async Task<IActionResult> UpdateStudentInfo([FromBody] StudentUpdateInfoDTO infoDTO)
        {
            var result = await _studentService.UpdateStudent(infoDTO);
            if (result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPost("create-relative")]
        public async Task<IActionResult> CreateRelative([FromBody] CreateRelativeDTO relativeDTO)
        {
            var result = await _studentService.CreateRelativesForStudent(relativeDTO);
            if (result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }
    }
}
