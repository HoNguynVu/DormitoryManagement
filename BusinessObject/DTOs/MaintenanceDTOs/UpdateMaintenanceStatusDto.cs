using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.MaintenanceDTOs
{
    public class UpdateMaintenanceStatusDto
    {
        public string RequestId { get; set; } = null!; // Mã yêu cầu bảo trì

        // Các trạng thái: "Pending", "Processing", "Completed", "Cancelled"
        public string NewStatus { get; set; } = null!;

        public string? ManagerNote { get; set; } // Ghi chú: "Đã thay bóng đèn", "SV làm vỡ"

        public decimal RepairCost { get; set; } = 0; // Phí sửa chữa (Mặc định là 0)
    }
}
