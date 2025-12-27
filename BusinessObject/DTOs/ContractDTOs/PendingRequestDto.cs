using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.ContractDTOs
{
    public class PendingRequestDto
    {
        public string ReceiptId { get; set; } = string.Empty;
        public DateTime ReceiptDate {get; set; }
        public int Months { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
