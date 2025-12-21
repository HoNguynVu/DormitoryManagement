using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.MaintenanceDTOs
{
    public class CreateMaintenanceDto
    {
        public string StudentId { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string EquipmentId { get; set; } = null!;
    }
}
