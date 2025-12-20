using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using BusinessObject.Base;

namespace BusinessObject.Entities;

[Table("BuildingManagers")]
public partial class BuildingManager : Person
{
    [Key]
    [Column("ManagerID")]
    [StringLength(128)]
    public string ManagerID { get; set; } = null!;

    [Required]
    [Column("AccountID")]
    [StringLength(128)]
    public string AccountID { get; set; } = null!;

    [ForeignKey("AccountID")]
    public Account Account { get; set; } = null!;

    [Required]
    [Column("CitizenID")]
    [StringLength(20)]
    public string CitizenId { get; set; } = null!;

    [Column(TypeName = "date")]
    public DateTime? DateOfBirth { get; set; }

    [Required]
    [StringLength(100)]
    public string Email { get; set; } = null!;

    // Navigation Properties
    [InverseProperty("Manager")]
    public virtual ICollection<Building> Buildings { get; set; } = new List<Building>();

    [InverseProperty("ReportingManager")]
    public virtual ICollection<Violation> ReportedViolations { get; set; } = new List<Violation>();
}
