using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    [Table("Hospitals")]
    public class Hospital
    {
        [Key]
        [StringLength(128)]
        public string HospitalID { get; set; }

        [Required]
        [StringLength(100)]
        public string HospitalName { get; set; }

        public virtual ICollection<HealthInsurance> Insurances { get; set; } = new List<HealthInsurance>();
    }
}
