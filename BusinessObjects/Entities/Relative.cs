using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

public partial class Relative
{
    [Key]
    [Column("RelativeID")]
    [StringLength(128)]
    public string RelativeId { get; set; } = null!;

    [Column("StudentID")]
    [StringLength(128)]
    public string StudentId { get; set; } = null!;

    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [StringLength(100)]
    public string? Occupation { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("Relatives")]
    public virtual Student Student { get; set; } = null!;
}
