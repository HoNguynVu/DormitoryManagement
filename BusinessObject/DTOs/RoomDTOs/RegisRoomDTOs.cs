using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.RoomDTOs
{
    public class RegisRoomDTOs
    {
        public string RoomId { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public int CurrentOccupancy { get; set; }
        public int RegisteredOccupancy { get; set; }
    }
}
