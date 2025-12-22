using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities
{
    [Table("HealthInsurances")]
    public class HealthInsurance
    {
        [Key]
        [StringLength(128)]
        public string InsuranceID { get; set; }

        [Required]
        [StringLength(128)]
        public string StudentID { get; set; }
        public virtual Student Student { get; set; }

        [Required]
        [StringLength(128)]
        public string HospitalID { get; set; }
        public virtual Hospital Hospital { get; set; }

        [StringLength(50)]
        public string CardNumber { get; set; } 

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Cost { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Mặc định là Pending

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
