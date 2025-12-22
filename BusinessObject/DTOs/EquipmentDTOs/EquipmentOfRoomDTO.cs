using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.EquipmentDTO
{
    public class EquipmentOfRoomDTO
    {
        public string EquipmentID { get; set; } = string.Empty;
        public string EquipmentName { get; set; } = string.Empty;
        public int Quantity { get; set; } 
        public string Status { get; set; } = string.Empty;
    }
}
