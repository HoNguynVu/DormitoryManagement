using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

public partial class HealthInsurance
{
    [Key]
    [Column("InsuranceID")]
    [StringLength(128)]
    public string InsuranceId { get; set; } = null!;

    [Column("StudentID")]
    [StringLength(128)]
    public string StudentId { get; set; } = null!;

    [StringLength(255)]
    public string? InitialHospital { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("HealthInsurances")]
    public virtual Student Student { get; set; } = null!;
}
