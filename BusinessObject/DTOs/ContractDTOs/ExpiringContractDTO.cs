using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.ContractDTOs
{
    public class ExpiringContractDTO
    {
        public string ContractID { get; set; } = string.Empty;
        public string StudentID { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string RoomID { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
    }
}
