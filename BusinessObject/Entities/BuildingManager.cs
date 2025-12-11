using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

[Index("CitizenId", Name = "UQ__Building__6E49FBED12B85B8D", IsUnique = true)]
[Index("Email", Name = "UQ__Building__A9D1053482D2A488", IsUnique = true)]
public partial class BuildingManager
{
    [Key]
    [Column("ManagerID")]
    [StringLength(128)]
    public string ManagerId { get; set; } = null!;

    [Column("UserID")]
    [StringLength(128)]
    public string UserId { get; set; } = null!;

    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [Column("CitizenID")]
    [StringLength(20)]
    public string CitizenId { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(100)]
    public string Email { get; set; } = null!;

    [StringLength(500)]
    public string? Address { get; set; }

    [InverseProperty("Manager")]
    public virtual ICollection<Building> Buildings { get; set; } = new List<Building>();

    [ForeignKey("UserId")]
    [InverseProperty("BuildingManagers")]
    public virtual Account User { get; set; } = null!;

    [InverseProperty("ReportingManager")]
    public virtual ICollection<Violation> Violations { get; set; } = new List<Violation>();
}
