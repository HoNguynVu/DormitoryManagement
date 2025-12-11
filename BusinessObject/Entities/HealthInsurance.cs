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
        [ForeignKey("StudentID")]
        public Student Student { get; set; }

        [StringLength(255)]
        public string InitialHospital { get; set; }
    }
}
