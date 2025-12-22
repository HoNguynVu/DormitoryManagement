using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.EquipmentDTOs
{
    public class ChangeStatusRoomEquipmentDto
    {
        public string RoomId { get; set; }  = string.Empty;
        public string EquipmentId { get; set; } = string.Empty;
        public int Quantity { get; set; } 
        public string FromStatus { get; set; } = string.Empty;
        public string ToStatus { get; set; } = string.Empty;
    }
}
