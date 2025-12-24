using API.Services.Interfaces;
using BusinessObject.DTOs.ContractDTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    // 1. Dùng danh từ số nhiều cho Resource
    [Route("api/contracts")]
    [ApiController]
    public class ContractController : ControllerBase
    {
        private readonly IContractService _contractService;

        public ContractController(IContractService contractService)
        {
            _contractService = contractService;
        }

        // ================= GET METHODS (READ) =================

        // GET: api/contracts
        // Gộp Filter và Overview vào luồng GET chính hoặc tách resource con nếu cần thiết.
        // Ở đây tôi giữ API filter làm endpoint chính để lấy danh sách.
        [HttpGet]
        public async Task<IActionResult> GetContracts(
            [FromQuery] string? keyword,
            [FromQuery] string? buildingName,
            [FromQuery] string? status)
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

        // GET: api/contracts/{id}
        // Lấy chi tiết hợp đồng theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContractById([FromRoute] string id)
        {
            // Lưu ý: Code cũ của bạn định nghĩa route "detail/{contractId}" nhưng lại dùng [FromQuery].
            // Chuẩn REST là dùng [FromRoute]
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

        // GET: api/contracts/overview
        // Tài nguyên thống kê (Singleton resource)
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

        // GET: api/contracts/pending-renewals
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
                receiptId = result.receiptId
            });
        }

        // GET: api/contracts/students/{studentId}/current
        // Hoặc: api/students/{studentId}/contracts/current (Nếu có StudentController thì nên để bên đó)
        // Ở đây giữ trong ContractController nhưng đặt route rõ ràng
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

        // POST: api/contracts/renewals
        // Tạo yêu cầu gia hạn (Tạo ra một resource "Renewal Request")
        [HttpPost("renewals")]
        public async Task<IActionResult> RequestRenewal([FromBody] RenewalRequestDto request)
        {
            var result = await _contractService.RequestRenewalAsync(request.StudentId, request.MonthsToExtend);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            // 201 Created là chuẩn cho POST tạo mới
            return StatusCode(result.StatusCode, new
            {
                message = "Renewal request created successfully.",
                receiptId = result.receiptId
            });
        }

        // PUT: api/contracts/{id}/extension
        // Xác nhận gia hạn (Cập nhật thời gian hợp đồng)
        [HttpPut("{id}/extension")]
        public async Task<IActionResult> ConfirmExtension([FromRoute] string id, [FromBody] ConfirmExtensionDto request)
        {
            if (request == null || request.MonthsAdded <= 0)
            {
                return BadRequest(new { message = "Số tháng gia hạn phải lớn hơn 0." });
            }

            var result = await _contractService.ConfirmContractExtensionAsync(id, request.MonthsAdded);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        // POST: api/contracts/rejections
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

        // DELETE: api/contracts/active?studentId=...
        // Chấm dứt hợp đồng. Dùng DELETE hợp lý hơn POST vì bản chất là hủy bỏ resource active.
        // Tuy nhiên, vì service nhận vào StudentId (chứ không phải ContractId), ta dùng Query String.
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

        // POST: api/contracts/room-transfers
        // Đổi phòng = Tạo ra một yêu cầu chuyển phòng (Transaction)
        [HttpPost("room-transfers")]
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

            return Ok(new { success = true, message = result.Message });
        }

        // POST: api/contracts/refunds
        // Xác nhận hoàn tiền
        [HttpPost("refunds")]
        public async Task<IActionResult> ConfirmRefund([FromBody] ConfirmRefundDto request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "Request body is required." });
            }

            var result = await _contractService.ConfirmRefundAsync(request);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }
    }
}