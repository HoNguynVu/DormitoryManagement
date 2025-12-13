using API.Services.Interfaces;
using BusinessObject.DTOs.ParameterDTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParameterController : ControllerBase
    {
        private readonly IParameterService _parameterService;
        public ParameterController(IParameterService parameterService)
        {
            _parameterService = parameterService;
        }

        [HttpPost("CreateNewParameter")]
        public async Task<IActionResult> CreateNewParameter([FromBody] CreateParameterDTO parameterDTO)
        {
            var result = await _parameterService.SetNewParameter(parameterDTO);
            if (result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpGet("GetAllParameters")]
        public async Task<IActionResult> GetAllParameters()
        {
            var result = await _parameterService.GetAllParameter();
            if (result.Success)
            {
                return StatusCode(result.StatusCode, new { data = result.listPara, message = result.Message });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }
    }
}
