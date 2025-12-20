using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublicInformationController : ControllerBase
    {
        private readonly IPublicInformationService _publicInformationService;
        public PublicInformationController(IPublicInformationService publicInformationService)
        {
            _publicInformationService = publicInformationService;
        }
        [HttpGet("Schools")]
        public async Task<IActionResult> GetSchools()
        {
            var result = await _publicInformationService.GetSchoolsAsync();
            if (result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message, schools = result.Schools });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }
        [HttpGet("Priorities")]
        public async Task<IActionResult> GetPriorities()
        {
            var result = await _publicInformationService.GetPrioritiesAsync();
            if (result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message, priorities = result.Priorities });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }
    }
}
