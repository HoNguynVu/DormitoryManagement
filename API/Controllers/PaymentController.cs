using API.Services.Interfaces;
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
        [HttpPost("create-zalopay-link/{registrationId}")]
        public async Task<IActionResult> CreateZaloPayLinkForRegistration(string registrationId)
        {
            var (statusCode, dto) = await _paymentService.CreateZaloPayLinkForRegistration(registrationId);
            return StatusCode(statusCode, dto);
        }
    }
}
