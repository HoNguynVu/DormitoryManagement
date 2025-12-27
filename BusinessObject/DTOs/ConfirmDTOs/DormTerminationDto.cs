using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.ConfirmDTOs
{
    public class DormTerminationDto
    {
        public string ContractCode { get; set; } = string.Empty;    
        public string StudentName { get; set; } = string.Empty;     
        public string StudentEmail { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;       
        public string BuildingName { get; set; } = string.Empty;    
        public string RoomName { get; set; } = string.Empty;        
        public DateOnly TerminationDate { get; set; }                 
    }
}
