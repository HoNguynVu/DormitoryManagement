using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.HealthInsuranceDTOs
{
    public class HealthInsuranceRequestDto
    {
        public string StudentId { get; set; } =  string.Empty;
        public string CardNumber {  get; set; } = string.Empty;
        public string HospitalId { get; set; } = string.Empty;
    }
}
