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

        // ================= GET METHODS (READ) =================

        // GET: api/contract
        [HttpGet]
        public async Task<IActionResult> GetContracts(
            [FromQuery] string? keyword,
            [FromQuery] string? buildingId,
            [FromQuery] string? status,
            [FromQuery] DateOnly? startDate,
            [FromQuery] DateOnly? endDate)
        {
            var result = await _contractService.GetContractFiltered(keyword, buildingId, status,startDate,endDate);
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

        // GET: api/contract/{id}
        // Lấy chi tiết hợp đồng theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContractById([FromRoute] string id)
        {
            var result = await _contractService.GetDetailContract(id);
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

        // GET: api/contract/overview
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

        // GET: api/contract/pending-renewals
        // Lấy yêu cầu gia hạn đang chờ (Resource con hoặc filter đặc biệt)
        [HttpGet("pending-renewals")]
        public async Task<IActionResult> GetPendingRequestRenew([FromQuery] string studentId)
        {
            var result = await _contractService.GetPendingRenewalRequestAsync(studentId);
            if (!result.Success)
                return StatusCode(result.StatusCode, new { message = result.Message });

            return StatusCode(result.StatusCode, new
            {
                message = result.Message,
                data = result.dto
            });
        }

        // GET: api/contracts/student/{studentId}/current
        [HttpGet("students/{studentId}/current")]
        public async Task<IActionResult> GetCurrentContractByStudent([FromRoute] string studentId)
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

        // GET: api/contracts/students/{accountId}
        // Lấy chi tiết hợp đồng dựa trên accountId (khác studentId?)
        [HttpGet("students/{accountId}")]
        public async Task<IActionResult> GetStudentContractDetail([FromRoute] string accountId)
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

        // ================= POST/PUT/PATCH METHODS (WRITE) =================

        // POST: api/contract/renewals
        // Tạo yêu cầu gia hạn (Tạo ra một resource "Renewal Request")
        [HttpPost("renewals")]
        public async Task<IActionResult> RequestRenewal([FromBody] RenewalRequestDto request)
        {
            var result = await _contractService.RequestRenewalAsync(request.StudentId, request.MonthsToExtend);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            string? paymentUrl = null;
            string? appTransId = null;

            // 201 Created là chuẩn cho POST tạo mới
            return StatusCode(result.StatusCode, new
            {
                message = "Renewal request created successfully.",
                receiptId = result.receiptId
            });
        }
        // POST: api/contract/rejections
        // Từ chối gia hạn. Đây là một hành động xử lý (Process).
        [HttpPost("rejections")]
        public async Task<IActionResult> RejectRenewal([FromBody] RejectRenewalDto dto)
        {
            var result = await _contractService.RejectRenewalAsync(dto);
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return Ok(new { message = result.Message });
        }

        // DELETE: api/contract/active?studentId=...
        [HttpDelete("active")]
        public async Task<IActionResult> TerminateContract([FromQuery] string studentId)
        {
            var result = await _contractService.TerminateContractNowAsync(studentId);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        // POST: api/contract/room-transfers
        // Đổi phòng = Tạo ra một yêu cầu chuyển phòng (Transaction)
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
        [HttpPost("remind-bulk")]
        public async Task<IActionResult> RemindExpiringContracts()
        {
            var result = await _contractService.RemindBulkExpiringAsync();
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return Ok(new { message = result.Message });
        }

        [HttpPost("remind-single/{studentId}")]
        public async Task<IActionResult> RemindSingleStudent([FromRoute] string studentId)
        {
            var result = await _contractService.RemindSingleStudentAsync(studentId);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return Ok(new { message = result.Message });
        }
    }
}