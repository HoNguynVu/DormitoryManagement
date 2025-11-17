using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

[Index("Username", Name = "UQ__Accounts__536C85E45C63AD51", IsUnique = true)]
[Index("Email", Name = "UQ__Accounts__A9D105342D5B7769", IsUnique = true)]
public partial class Account
{
    [Key]
    [Column("UserID")]
    [StringLength(128)]
    public string UserId { get; set; } = null!;

    [StringLength(100)]
    public string Username { get; set; } = null!;

    [StringLength(255)]
    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    [StringLength(20)]
    public string Role { get; set; } = null!;

    public bool IsActive { get; set; }

    public bool IsEmailVerified { get; set; }

    public DateTime CreatedAt { get; set; }

    [InverseProperty("User")]
    public virtual BuildingManager? BuildingManager { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<OtpCode> OtpCodes { get; set; } = new List<OtpCode>();

    [InverseProperty("User")]
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    [InverseProperty("User")]
    public virtual Student? Student { get; set; }
}
