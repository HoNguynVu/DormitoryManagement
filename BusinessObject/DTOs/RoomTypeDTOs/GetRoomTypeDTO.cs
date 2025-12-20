using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.RoomTypeDTOs
{
    public class GetRoomTypeDTO
    {
        public string RoomTypeID { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
    }
}
