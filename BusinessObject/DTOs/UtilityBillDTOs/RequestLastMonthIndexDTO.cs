using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.UtilityBillDTOs
{
    public class RequestLastMonthIndexDTO
    {
        public string RoomId { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
