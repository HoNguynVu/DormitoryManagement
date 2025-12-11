using API.Services.Interfaces;
using BusinessObject.DTOs.ContractDTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractController : ControllerBase
    {
        private readonly IContractService _contractService;
        public ContractController(IContractService contractService)
        {
            _contractService = contractService;
        }
        // POST: api/contracts/renewal-request
        // Sinh viên yêu cầu gia hạn 
        [HttpPost("renewal-request")]
        public async Task<IActionResult> RequestRenewal([FromBody] RenewalRequestDto request)
        {
            var result = await _contractService.RequestRenewalAsync(request.StudentId, request.MonthsToExtend);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            return StatusCode(result.StatusCode, new
            {
                message = "Renewal request created successfully.",
                invoiceId = result.Message
            });
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetCurrentContract(string studentId)
        {
            var result = await _contractService.GetCurrentContractAsync(studentId);

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

        [HttpPost("terminate/{studentId}")]
        public async Task<IActionResult> TerminateContract(string studentId)
        {
            var result = await _contractService.TerminateContractNowAsync(studentId);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

    }
}
