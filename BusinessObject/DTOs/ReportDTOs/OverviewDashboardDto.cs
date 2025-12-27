using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.ReportDTOs
{
    public class OverviewDashBoardDto
    {
        public int TotalStudents { get; set; }
        public decimal RateStudent { get; set; }
        public int TotalBuilding { get; set; }
        public int TotalManager { get; set; }
        public decimal RateManager { get; set; }
        public decimal TotalRevenue { get; set; }

    }
}
