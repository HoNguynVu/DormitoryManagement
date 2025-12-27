using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.ConfirmDTOs
{
    public class HealthInsurancePurchaseDto : BaseConfirmDto
    {
        public int Year { get; set; }  // Giai đoạn bảo hiểm (VD: 2024 - 2025)
        public DateOnly CoverageStartDate { get; set; } // Ngày hiệu lực
        public DateOnly CoverageEndDate { get; set; }  // Ngày hết hạn
        public decimal Cost { get; set; }            // Số tiền đã đóng
    }
}
