using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.HealthInsuranceDTOs
{
    public class SummaryHealthDto
    {
        public string HealthInsuranceId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatAt { get; set; } 
        public string HospitalName { get; set; } = string.Empty;
    }
}
