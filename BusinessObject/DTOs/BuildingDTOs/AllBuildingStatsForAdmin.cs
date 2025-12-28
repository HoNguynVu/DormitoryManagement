using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.BuildingDTOs
{
    public class AllBuildingStatsForAdmin
    {
        public int TotalRooms { get; set; }
        public int TotalAvailableRooms { get; set; }
        public int TotalFullRooms { get; set; }
        public int TotalMaintenanceRooms { get; set; }
    }
}
