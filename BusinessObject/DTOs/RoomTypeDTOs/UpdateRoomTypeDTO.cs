using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.RoomTypeDTOs
{
    public class UpdateRoomTypeDTO
    {
        public string TypeID { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Capacity { get; set; }  
        public decimal Price { get; set; }
    }
}
