using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.MaintenanceDTOs
{
    public class DetailMaintenanceDto : SummaryMaintenanceDto
    {
        public decimal? Amount { get; set; }
        public string? ManagerNote { get; set; }
    }
}
