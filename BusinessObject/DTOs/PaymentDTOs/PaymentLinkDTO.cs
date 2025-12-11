using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.PaymentDTOs
{
    public class PaymentLinkDTO
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? PaymentId { get; set; }
        public string? PaymentUrl { get; set; }
    }
}
