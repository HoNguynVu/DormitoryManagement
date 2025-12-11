using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

[Index("RoomName", Name = "UQ__Rooms__6B500B55908FD691", IsUnique = true)]
public partial class Room
{
    [Key]
    [Column("RoomID")]
    [StringLength(128)]
    public string RoomId { get; set; } = null!;

    [Column("BuildingID")]
    [StringLength(128)]
    public string BuildingId { get; set; } = null!;

    [Column("RoomTypeID")]
    [StringLength(128)]
    public string? RoomTypeId { get; set; }

    [StringLength(100)]
    public string RoomName { get; set; } = null!;

    public int Capacity { get; set; }

    public int CurrentOccupancy { get; set; }

    [StringLength(20)]
    public string RoomStatus { get; set; } = null!;

    public bool IsUnderMaintenance { get; set; }

    public bool IsBeingCleaned { get; set; }

    [ForeignKey("BuildingId")]
    [InverseProperty("Rooms")]
    public virtual Building Building { get; set; } = null!;

    [InverseProperty("Room")]
    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    [InverseProperty("Room")]
    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

    [InverseProperty("Room")]
    public virtual ICollection<RegistrationForm> RegistrationForms { get; set; } = new List<RegistrationForm>();

    [ForeignKey("RoomTypeId")]
    [InverseProperty("Rooms")]
    public virtual RoomType? RoomType { get; set; }

    [InverseProperty("Room")]
    public virtual ICollection<UtilityBill> UtilityBills { get; set; } = new List<UtilityBill>();
}
