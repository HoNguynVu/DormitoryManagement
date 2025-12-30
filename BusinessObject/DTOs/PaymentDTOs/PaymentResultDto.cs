using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.PaymentDTOs
{
    public class PaymentResultDto
    {
        public decimal Amount { get; set; }
        public string TransId { get; set; } = string.Empty;
        public DateTime PaymentTime { get; set; }

        public string Description {get;set;} = string.Empty;
        public string ReceiptId { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public string RelatedId { get; set; } = string.Empty;
    }
}
