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
        [HttpPost("create-zalopay-link/registration/{registrationId}")]
        public async Task<IActionResult> CreateZaloPayLinkForRegistration(string registrationId)
        {
            var (statusCode, dto) = await _paymentService.CreateZaloPayLinkForRegistration(registrationId);
            return StatusCode(statusCode, dto);
        }

        [HttpPost("create-zalopay-link/renewal-contract/{receiptId}")]
        public async Task<IActionResult> CreateZaloPayLinkForContract(string receiptId)
        {
            var (statusCode, dto) = await _paymentService.CreateZaloPayLinkForRenewal(receiptId);
            return StatusCode(statusCode, dto);
        }

        [HttpPost("create-zalopay-link/utility/{utilityId}")]
        public async Task<IActionResult> CreateZaloPayLinkForUtility(string utilityId, string accountId)
        {
            var (statusCode, dto) = await _paymentService.CreateZaloPayLinkForUtility(utilityId, accountId);
            return StatusCode(statusCode, dto);
        }

        [HttpPost("create-zalopay-link/health-insurance/{insuranceId}")]
        public async Task<IActionResult> CreateZaloPayLinkForHealthInsurance(string insuranceId)
        {
            var (statusCode, dto) = await _paymentService.CreateZaloPayLinkForHealthInsurance(insuranceId);
            return StatusCode(statusCode, dto);
        }

        [HttpPost("create-zalopay-link/room-change/{receiptId}")]
        public async Task<IActionResult> CreateZaloPayLinkForRoomChange(string receiptId)
        {
            var (statusCode, dto) = await _paymentService.CreateZaloPayLinkForRoomChange(receiptId);
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
