using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

public partial class UtilityBill
{
    [Key]
    [Column("BillID")]
    [StringLength(128)]
    public string BillId { get; set; } = null!;

    [Column("RoomID")]
    [StringLength(128)]
    public string RoomId { get; set; } = null!;

    public int ElectricityUsage { get; set; }

    public int WaterUsage { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = null!;

    [ForeignKey("RoomId")]
    [InverseProperty("UtilityBills")]
    public virtual Room Room { get; set; } = null!;
}
