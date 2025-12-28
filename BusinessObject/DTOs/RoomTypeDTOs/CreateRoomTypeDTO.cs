using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.RoomTypeDTOs
{
    public class CreateRoomTypeDTO
    {
        public string TypeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Capacity { get; set; }
    }
}
