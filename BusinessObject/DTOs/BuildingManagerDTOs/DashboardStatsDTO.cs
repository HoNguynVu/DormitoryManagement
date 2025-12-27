using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.BuildingManagerDTOs
{
    public class DashboardStatsDTO
    {
        public int TotalStudents { get; set; }
        public int AvailableRooms { get; set; }
        public int CountRooms { get; set; }
        public int UnpaidUtilityBills { get; set; }
        public decimal TotalUnpaidAmount { get; set; }
        public int UnResolveRequests { get; set; }
    }
}
