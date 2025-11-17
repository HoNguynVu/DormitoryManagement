using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

public partial class RegistrationForm
{
    [Key]
    [Column("FormID")]
    [StringLength(128)]
    public string FormId { get; set; } = null!;

    [Column("StudentID")]
    [StringLength(128)]
    public string StudentId { get; set; } = null!;

    [Column("RoomID")]
    [StringLength(128)]
    public string RoomId { get; set; } = null!;

    public DateTime RegistrationTime { get; set; }

    [StringLength(30)]
    public string Status { get; set; } = null!;

    [ForeignKey("RoomId")]
    [InverseProperty("RegistrationForms")]
    public virtual Room Room { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("RegistrationForms")]
    public virtual Student Student { get; set; } = null!;
}
