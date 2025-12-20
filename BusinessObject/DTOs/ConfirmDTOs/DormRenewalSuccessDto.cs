using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.ConfirmDTOs
{
    public class DormRenewalSuccessDto : BaseConfirmDto
    {
        public string ContractCode { get; set; } = string.Empty; // Mã hợp đồng 
        public string BuildingName { get; set; } = string.Empty;    // Tòa nhà hiện tại
        public string RoomName { get; set; } = string.Empty;      // Phòng hiện tại
        public DateOnly OldEndDate { get; set; }   // Ngày hết hạn cũ 
        public DateOnly NewStartDate { get; set; }   // Ngày bắt đầu kỳ gia hạn mới
        public DateOnly NewEndDate { get; set; }     // Ngày hết hạn mới
        public decimal TotalAmountPaid { get; set; } // Số tiền đã thanh toán để gia hạn
    }
}
