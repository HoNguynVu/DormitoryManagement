using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.UtilityBillDTOs
{
    public class ManagerGetBillDTO
    {
        public string RoomID { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public int ElectricityOldIndex { get; set; } // Chỉ số điện cũ
        public int ElectricityNewIndex { get; set; } // Chỉ số điện mới

        public int WaterOldIndex { get; set; }       // Chỉ số nước cũ
        public int WaterNewIndex { get; set; }
        public int ElectricityUsage { get; set; }
        public int WaterUsage { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
