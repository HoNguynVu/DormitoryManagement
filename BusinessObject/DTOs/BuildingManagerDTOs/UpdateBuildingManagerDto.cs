using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.BuildingManagerDTOs
{
    public class UpdateBuildingManagerDto
    {
        public string ManagerID { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string CitizenId { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
    }
}
