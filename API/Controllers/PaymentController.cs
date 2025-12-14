using API.Services.Interfaces;
using BusinessObject.DTOs.PaymentDTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }
        [HttpPost("create-zalopay-link/regis/{registrationId}")]
        public async Task<IActionResult> CreateZaloPayLinkForRegistration(string registrationId)
        {
            var (statusCode, dto) = await _paymentService.CreateZaloPayLinkForRegistration(registrationId);
            return StatusCode(statusCode, dto);
        }

        [HttpPost("create-zalopay-link/renew/{receiptId}")]
        public async Task<IActionResult> CreateZaloPayLinkForContract(string receiptId)
        {
            var (statusCode, dto) = await _paymentService.CreateZaloPayLinkForRenewal(receiptId);
            return StatusCode(statusCode, dto);
        }

        [HttpPost("callback")]
        public async Task<IActionResult> ZaloPayCallback([FromBody] ZaloPayCallbackDTO cbData)
        {
            var result = await _paymentService.ProcessZaloPayCallback(cbData);
            return Ok(new
            {
                return_code = result.ReturnCode,
                return_message = result.ReturnMessage
            });
        }
    }
}
