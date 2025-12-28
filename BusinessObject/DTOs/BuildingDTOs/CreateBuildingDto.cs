using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.BuildingDTOs
{
    public class CreateBuildingDto
    {
        public string BuildingName { get; set; } = string.Empty;
        public string ManagerID { get; set; } = string.Empty;
    }
}
