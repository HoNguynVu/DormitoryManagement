using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    [Table("HealthInsurancePrices")]
    public class HealthInsurancePrice
    {
        [Key]
        [StringLength(128)]
        public string HealthPriceID { get; set; } 

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; } 

        [Required]       
        public int Year { get; set; } 

        [Required]
        public DateOnly EffectiveDate { get; set; }
        public bool IsActive { get; set; } = true; 

        public virtual ICollection<HealthInsurance> Insurances { get; set; } = new List<HealthInsurance>();

    }
}
