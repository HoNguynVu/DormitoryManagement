using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

public partial class Violation
{
    [Key]
    [Column("ViolationID")]
    [StringLength(128)]
    public string ViolationId { get; set; } = null!;

    [Column("StudentID")]
    [StringLength(128)]
    public string StudentId { get; set; } = null!;

    [Column("ReportingManagerID")]
    [StringLength(128)]
    public string? ReportingManagerId { get; set; }

    [StringLength(255)]
    public string ViolationAct { get; set; } = null!;

    public DateTime ViolationTime { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(255)]
    public string? Resolution { get; set; }

    [ForeignKey("ReportingManagerId")]
    [InverseProperty("Violations")]
    public virtual BuildingManager? ReportingManager { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("Violations")]
    public virtual Student Student { get; set; } = null!;
}
