using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.ConfirmDTOs
{
    public class UtilityPaymentSuccessDto : BaseConfirmDto
    {
        public string ReceiptID { get; set; } = string.Empty;     // Mã hóa đơn
        public string BuildingName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string BillingMonth { get; set; } = string.Empty;  // Tháng thanh toán 
        public DateTime PaymentDate { get; set; }    // Ngày thanh toán

        // Chỉ số điện
        public int ElectricIndexOld { get; set; }    // Chỉ số đầu
        public int ElectricIndexNew { get; set; }    // Chỉ số cuối
        public int ElectricUsage { get; set; }       // Số điện tiêu thụ
        public decimal ElectricAmount { get; set; }  // Tiền điện

        // Chỉ số nước
        public int WaterIndexOld { get; set; }
        public int WaterIndexNew { get; set; }
        public int WaterUsage { get; set; }
        public decimal WaterAmount { get; set; }

        // Tổng cộng
        public decimal TotalAmount { get; set; }     // Tổng tiền đã thanh toán
    }
}
