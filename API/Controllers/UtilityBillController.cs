using API.Services.Interfaces;
using BusinessObject.DTOs.UtilityBillDTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtilityBillController : ControllerBase
    {
        private readonly IUtilityBillService utilityBillService;
        public UtilityBillController(IUtilityBillService utilityBillService)
        {
            this.utilityBillService = utilityBillService;
        }

        [HttpGet("by-student/{accountId}")]
        public async Task<IActionResult> GetUtilityBillsByStudent(string accountId)
        {
            var result = await utilityBillService.GetUtilityBillsByStudent(accountId);
            if (result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message, data = result.listBill });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUtilityBill([FromBody] CreateBillDTO dto)
        {
            var result = await utilityBillService.CreateUtilityBill(dto);
            if (result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPost("by-manager")]
        public async Task<IActionResult> GetUtilityBillsByManager([FromBody] ManagerGetBillRequest request)
        {
            var result = await utilityBillService.GetBillsForManagerAsync(request);
            if (result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message, data = result.listBill });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpGet("active-parameter")]
        public async Task<IActionResult> GetActiveParameter()
        {
            var result = await utilityBillService.GetActiveParameter();
            if (result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message, data = result.para });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }
    }
}
