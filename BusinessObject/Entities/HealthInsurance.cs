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

    [StringLength(50)]
    public string? CardNumber { get; set; }

    [StringLength(255)]
    public string InitialHospital { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Cost { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("HealthInsurances")]
    public virtual Student Student { get; set; } = null!;
}
