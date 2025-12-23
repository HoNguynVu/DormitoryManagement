using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.HealthInsuranceDTOs
{
    public class CreateHealthPriceDTO
    {
        public decimal Amount { get; set; }

        public int Year { get; set; } 

        public DateOnly EffectiveDate { get; set; }
    }
}
