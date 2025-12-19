using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

[Table("Rooms")]
public partial class Room
{
    [Key]
    [Column("RoomID")]
    [StringLength(128)]
    public string RoomID { get; set; } = null!;

    [Column("BuildingID")]
    [StringLength(128)]
    public string BuildingID { get; set; } = null!;

    [Column("RoomTypeID")]
    [StringLength(128)]
    public string? RoomTypeID { get; set; }

    [StringLength(100)]
    public string RoomName { get; set; } = null!;

    public int Capacity { get; set; }

    public int CurrentOccupancy { get; set; }

    [StringLength(20)]
    public string RoomStatus { get; set; } = null!;

    public bool IsUnderMaintenance { get; set; }

    public bool IsBeingCleaned { get; set; }
    public string Genre { get; set; } = null!;

    [ForeignKey("BuildingID")]
    [InverseProperty("Rooms")]
    public virtual Building Building { get; set; } = null!;

    [InverseProperty("Room")]
    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    [InverseProperty("Room")]
    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

    [InverseProperty("Room")]
    public virtual ICollection<RegistrationForm> RegistrationForms { get; set; } = new List<RegistrationForm>();

    [ForeignKey("RoomTypeID")]
    [InverseProperty("Rooms")]
    public virtual RoomType? RoomType { get; set; }

    [InverseProperty("Room")]
    public virtual ICollection<UtilityBill> UtilityBills { get; set; } = new List<UtilityBill>();

    /// <summary>
    /// Assign a new RoomType to this Room. This updates the Capacity from the RoomType
    /// and validates that the new capacity is not less than the current occupancy.
    /// </summary>
    /// <param name="newType">RoomType to assign</param>
    /// <exception cref="ArgumentNullException">If newType is null</exception>
    /// <exception cref="InvalidOperationException">If newType.Capacity is less than CurrentOccupancy</exception>
    public void AssignRoomType(RoomType newType)
    {
        if (newType == null) throw new ArgumentNullException(nameof(newType));

        if (newType.Capacity < this.CurrentOccupancy)
            throw new InvalidOperationException("New room type capacity is less than current occupancy");

        this.RoomTypeID = newType.RoomTypeID;
        this.Capacity = newType.Capacity;
        this.RoomType = newType;
    }

    public virtual ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
}
