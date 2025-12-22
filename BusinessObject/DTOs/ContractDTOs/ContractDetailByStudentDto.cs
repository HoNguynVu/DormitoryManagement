using BusinessObject.DTOs.EquipmentDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.ContractDTOs
{
    public class ContractDetailByStudentDto
    {
        public string ContractID { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;// Active, Pending, Expired, Cancelled
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public string ManagerID { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public string ManagerPhone { get; set; } = string.Empty;
        public string ManagerEmail { get; set; } = string.Empty;

        public string RoomName { get; set; } = string.Empty;      
        public string BuildingName { get; set; } = string.Empty;  
        public string RoomTypeName { get; set; } = string.Empty;    
        public decimal RoomPrice { get; set; }
        
        public List<EquipmentOfRoomDTO> Equipments { get; set; } = new List<EquipmentOfRoomDTO>();
    }
}
