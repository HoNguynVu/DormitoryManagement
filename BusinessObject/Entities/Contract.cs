using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

public partial class Contract
{
    [Key]
    [Column("ContractID")]
    [StringLength(128)]
    public string ContractId { get; set; } = null!;

    [Column("StudentID")]
    [StringLength(128)]
    public string StudentId { get; set; } = null!;

    [Column("RoomID")]
    [StringLength(128)]
    public string RoomId { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [StringLength(30)]
    public string ContractStatus { get; set; } = null!;

    [ForeignKey("RoomId")]
    [InverseProperty("Contracts")]
    public virtual Room Room { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("Contracts")]
    public virtual Student Student { get; set; } = null!;
}
