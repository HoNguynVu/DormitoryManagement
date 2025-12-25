using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.HealthInsuranceDTOs
{   
    public class HealthPriceDto
    {
        public string HealthPriceId { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Year { get; set; }
        public bool IsActive { get; set; }

    }
}
