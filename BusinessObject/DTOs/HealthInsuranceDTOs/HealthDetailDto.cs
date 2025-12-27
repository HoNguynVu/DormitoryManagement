using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.HealthInsuranceDTOs
{
    public class HealthDetailDto : SummaryHealthDto
    {
        public string Email { get; set; } = string.Empty;
        public decimal Price { get; set; }
        
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }
}
