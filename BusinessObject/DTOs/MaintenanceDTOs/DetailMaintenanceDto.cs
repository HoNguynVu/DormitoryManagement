using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.MaintenanceDTOs
{
    public class DetailMaintenanceDto : SummaryMaintenanceDto
    {
        public string? ManagerNote { get; set; }
    }
}
