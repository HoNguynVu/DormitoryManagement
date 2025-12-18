using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.ContractDTOs
{
    public class SummaryContractDto
    {
        public string ContractID { get; set; } = string.Empty;
        public string StudentID { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        
        public string RoomName { get; set; } = string.Empty;
        public string BuildingName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int RemainingDays { get; set; }
        public string Status {  get; set; } = string.Empty ;
    }
}
