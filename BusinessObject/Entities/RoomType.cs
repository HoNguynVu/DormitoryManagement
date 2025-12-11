using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

[Index("TypeName", Name = "UQ__RoomType__D4E7DFA8E7A551A9", IsUnique = true)]
public partial class RoomType
{
    [Key]
    [Column("RoomTypeID")]
    [StringLength(128)]
    public string RoomTypeId { get; set; } = null!;

    [StringLength(100)]
    public string TypeName { get; set; } = null!;

    public int Capacity { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [InverseProperty("RoomType")]
    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
