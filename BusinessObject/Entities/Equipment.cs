using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

public partial class Equipment
{
    [Key]
    [Column("EquipmentID")]
    [StringLength(128)]
    public string EquipmentId { get; set; } = null!;

    [Column("RoomID")]
    [StringLength(128)]
    public string RoomId { get; set; } = null!;

    [StringLength(100)]
    public string EquipmentName { get; set; } = null!;

    [StringLength(100)]
    public string? Status { get; set; }

    [ForeignKey("RoomId")]
    [InverseProperty("Equipment")]
    public virtual Room Room { get; set; } = null!;
}
