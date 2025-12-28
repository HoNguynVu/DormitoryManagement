using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.ReportDTOs
{
    public class BuildingPerformanceDto
    {
        public string BuildingId { get; set; } = string.Empty;
        public string BuildingName { get; set; } = string.Empty;
        public int TotalBeds { get; set; }
        public int UsedBeds { get; set; }
        public double OccupancyRate { get; set; } 
        public decimal MonthlyRevenue { get; set; }
    }
}
