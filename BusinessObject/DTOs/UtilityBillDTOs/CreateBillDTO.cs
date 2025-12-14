using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.UtilityBillDTOs
{
    public class CreateBillDTO
    {
        public string RoomId { get; set; }
        public int ElectricityIndex { get; set; }
        public int WaterIndex { get; set; }
    }
}
