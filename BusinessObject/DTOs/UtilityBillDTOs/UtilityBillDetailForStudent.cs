using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.UtilityBillDTOs
{
    public class UtilityBillDetailForStudent
    {
        public string BillId { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public int ElectricityOldIndex { get; set; }
        public int ElectricityNewIndex { get;set; }
        public int WaterOldIndex { get; set; }     
        public int WaterNewIndex { get; set; }
        public int ElectricityUsage { get; set; }
        public decimal ElectricityUnitPrice { get; set; }
        public decimal WaterUnitPrice { get; set; }
        public int WaterUsage { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
