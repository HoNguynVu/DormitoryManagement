using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.MaintenanceDTOs
{
    public class SummaryMaintenanceDto
    {
        public string MaintenanceID { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string EquipmentName { get; set; } = string.Empty;
        public string Description {  get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;  
        public DateOnly MaintenanceDate {  get; set; }
    }
}
