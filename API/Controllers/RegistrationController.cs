using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using BusinessObject.DTOs.RegisDTOs;
using BusinessObject.Entities;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;
        public RegistrationController(IRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateRegistrationForm([FromBody] RegistrationFormRequest registrationForm)
        {
            var (success, message, statusCode, registrationId) = await _registrationService.CreateRegistrationForm(registrationForm);
            if (success)
            {
                return StatusCode(statusCode, new { Message = message, registrationId = registrationId });
            }
            else
            {
                return StatusCode(statusCode, new { Error = message });
            }
        }

    }
}
