using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.ViolationDTOs
{
    public class ViolationStats
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string RoomId { get; set; }
        public string RoomName { get; set; }
        public int TotalViolations { get; set; }
    }
}
