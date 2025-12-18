using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.ConfirmDTOs
{
    public class DormRegistrationSuccessDto : BaseConfirmDto
    {
        public string ContractCode { get; set; } = string.Empty;    // Mã hợp đồng
        public string BuildingName { get; set; } = string.Empty;    // Tên tòa nhà 
        public string RoomName { get; set; } = string.Empty;  // Số phòng 
        public string RoomType { get; set; } = string.Empty;      // Loại phòng 
        public DateOnly StartDate { get; set; }      // Ngày bắt đầu ở
        public DateOnly EndDate { get; set; }        // Ngày kết thúc
        public decimal DepositAmount { get; set; }  // Số tiền đã đóng
        public decimal RoomFeePerMonth { get; set; } // Giá phòng hàng tháng
    }
}
