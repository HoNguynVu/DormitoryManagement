using API.Services.Implements;
using API.Services.Interfaces;
using BusinessObject.DTOs.ContractDTOs;
using Microsoft.AspNetCore.Mvc;
using MimeKit.Cryptography;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly IPaymentService _paymentService;
        public ContractController(IContractService contractService, IPaymentService paymentService)
        {
            _contractService = contractService;
            _paymentService = paymentService;
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

        [HttpPut("confirm-extension/{contractId}")]
        public async Task<IActionResult> ConfirmExtension(string contractId, [FromBody] ConfirmExtensionDto request)
        {
            // 1. Validation 
            if (request == null || request.MonthsAdded <= 0)
            {
                return BadRequest(new { message = "Số tháng gia hạn phải lớn hơn 0." });
            }

            // 2. Gọi Service
            var result = await _contractService.ConfirmContractExtensionAsync(contractId, request.MonthsAdded);

            // 3. Xử lý kết quả trả về từ Tuple (Success, Message, StatusCode)
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            // 4. Thành công (200 OK)
            return Ok(new { message = result.Message });
        }

        // POST: api/contracts/change-room
        // Trưởng tòa thực hiện đổi phòng cho sinh viên
        [HttpPost("change-room")]
        public async Task<IActionResult> ChangeRoom([FromBody] ChangeRoomRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "Request body is required." });
            }

            var result = await _contractService.ChangeRoomAsync(request);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.Message });
            }
            string? paymentUrl = null;
            string? appTransId = null;

            if (result.Type == "Charge" && !string.IsNullOrEmpty(result.ReceiptId))
            {
                var paymentRes = await _paymentService.CreateZaloPayLinkForRoomChange(result.ReceiptId);

                if (paymentRes.StatusCode == 200 && paymentRes.dto.IsSuccess)
                {
                    paymentUrl = paymentRes.dto.PaymentUrl;
                    appTransId = paymentRes.dto.PaymentId;
                }
                else
                {
                    // Lỗi khi gọi ZaloPay (nhưng Receipt đã tạo rồi)
                    return StatusCode(paymentRes.StatusCode, new
                    {
                        success = true,
                        message = "Tạo yêu cầu đổi phòng thành công nhưng lỗi tạo link thanh toán.",
                        paymentError = paymentRes.dto.Message,
                        receiptId = result.ReceiptId
                    });
                }
            }

            // BƯỚC 3: Trả về kết quả cho FE
            return Ok(new
            {
                success = true,
                message = result.Message,
                receiptId = result.ReceiptId,
                type = result.Type, // "Charge", "Refund", "None"
                paymentUrl = paymentUrl,
                appTransId = appTransId
            });
        }

        [HttpGet("detail/{contractId}")]
        public async Task<IActionResult> GetDetailContract(string contractId)
        {
            var result = await _contractService.GetDetailContract(contractId);
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return Ok(new
            {
                message = result.Message,
                data = result.dto
            });

        }

        [HttpGet("filtered")]
        public async Task<IActionResult> GetContractFiltered([FromQuery] string? keyword, [FromQuery] string? buildingName, [FromQuery] string? status)
        {
            var result = await _contractService.GetContractFiltered(keyword, buildingName, status);
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return Ok(new
            {
                message = result.Message,
                data = result.dto
            });
        }
        [HttpGet("overview")]
        public async Task<IActionResult> GetContractOverview()
        {
            var result = await _contractService.GetOverviewContract();
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return Ok(new
            {
                message = result.Message,
                data = result.stat
            });
        }

        [HttpPost("reject-renewal")]
        public async Task<IActionResult> RejectRenewal([FromBody] RejectRenewalDto dto)
        {
            var result = await _contractService.RejectRenewalAsync(dto);
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return Ok(new { message = result.Message });
        }

        [HttpGet("student-detail/{accountId}")]
        public async Task<IActionResult> GetStudentContractDetail(string accountId)
        {
            var result = await _contractService.GetContractDetailByStudentAsync(accountId);
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return Ok(new
            {
                message = result.Message,
                data = result.dto
            });
        }
    }
}
