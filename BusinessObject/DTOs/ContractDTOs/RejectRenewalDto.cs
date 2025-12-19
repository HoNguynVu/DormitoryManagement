using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.ContractDTOs
{
    public class RejectRenewalDto
    {
        public string ReceiptId { get; set; } = string.Empty;  // Mã biên lai yêu cầu gia hạn hợp đồng
        public string Reason { get; set; } = string.Empty;     // Lý do từ chối gia hạn
    }
}
