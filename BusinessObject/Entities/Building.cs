using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

[Index("BuildingName", IsUnique = true)]
public partial class Building
{
    [Key]
    [Column("BuildingID")]
    [StringLength(128)]
    public string BuildingId { get; set; } = null!;

    [StringLength(100)]
    public string BuildingName { get; set; } = null!;

    [Column("ManagerID")]
    [StringLength(128)]
    public string? ManagerId { get; set; }

    [ForeignKey("ManagerId")]
    [InverseProperty("Buildings")]
    public virtual BuildingManager? Manager { get; set; }

    [InverseProperty("Building")]
    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
