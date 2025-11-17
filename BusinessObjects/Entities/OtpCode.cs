using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

public partial class OtpCode
{
    [Key]
    [Column("OtpID")]
    [StringLength(128)]
    public string OtpId { get; set; } = null!;

    [Column("UserID")]
    [StringLength(128)]
    public string UserId { get; set; } = null!;

    [StringLength(10)]
    public string Code { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    [StringLength(50)]
    public string Purpose { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("OtpCodes")]
    public virtual Account User { get; set; } = null!;
}
