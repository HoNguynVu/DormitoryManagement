using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

[Index("CitizenId", Name = "UQ__Students__6E49FBED608E0586", IsUnique = true)]
[Index("Email", Name = "UQ__Students__A9D10534F59F7243", IsUnique = true)]
public partial class Student
{
    [Key]
    [Column("StudentID")]
    [StringLength(128)]
    public string StudentId { get; set; } = null!;

    [Column("UserID")]
    [StringLength(128)]
    public string? UserId { get; set; }

    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [Column("CitizenID")]
    [StringLength(20)]
    public string CitizenId { get; set; } = null!;

    [Column("CitizenIDIssuePlace")]
    [StringLength(255)]
    public string? CitizenIdissuePlace { get; set; }

    [StringLength(500)]
    public string? CurrentAddress { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(100)]
    public string Email { get; set; } = null!;

    [Column("SchoolID")]
    [StringLength(128)]
    public string? SchoolId { get; set; }

    [Column("PriorityID")]
    [StringLength(128)]
    public string? PriorityId { get; set; }

    [InverseProperty("Student")]
    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    [InverseProperty("Student")]
    public virtual ICollection<HealthInsurance> HealthInsurances { get; set; } = new List<HealthInsurance>();

    [ForeignKey("PriorityId")]
    [InverseProperty("Students")]
    public virtual Priority? Priority { get; set; }

    [InverseProperty("Student")]
    public virtual ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();

    [InverseProperty("Student")]
    public virtual ICollection<RegistrationForm> RegistrationForms { get; set; } = new List<RegistrationForm>();

    [InverseProperty("Student")]
    public virtual ICollection<Relative> Relatives { get; set; } = new List<Relative>();

    [ForeignKey("SchoolId")]
    [InverseProperty("Students")]
    public virtual School? School { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Students")]
    public virtual Account? User { get; set; }

    [InverseProperty("Student")]
    public virtual ICollection<Violation> Violations { get; set; } = new List<Violation>();
}
