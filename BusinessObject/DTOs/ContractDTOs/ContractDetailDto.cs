using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.ContractDTOs
{
    public class DetailContractDto
    {
        // --- 1. Thông tin chính của Hợp đồng ---
        public string ContractID { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;// Active, Pending, Expired, Cancelled
        public int DaysRemaining { get; set; }     // Còn bao nhiêu ngày nữa hết hạn
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        // --- 2. Thông tin Sinh viên (Người đứng tên) ---
        public string StudentID { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentPhone { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;

        // --- 3. Thông tin Phòng & Vị trí ---
        public string RoomName { get; set; } = string.Empty;      // VD: Phòng 101
        public string BuildingName { get; set; } = string.Empty;  // VD: Tòa A
        public string RoomTypeName { get; set; } = string.Empty;  // VD: Phòng 4 người, Phòng dịch vụ
        public int MaxCapacity { get; set; }       // Sức chứa tối đa của phòng

        // --- 4. Thông tin Tài chính ---
        public decimal RoomPrice { get; set; }     // Giá thuê hàng tháng
    }
}
