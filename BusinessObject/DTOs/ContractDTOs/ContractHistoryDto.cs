using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.ContractDTOs
{
    public class ContractHistoryDto
    {
        public string ReceiptId { get; set; } = null!;
        public DateTime PrintTime { get; set; }
        public string Content { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Status { get; set; } = null!;
    }
}
