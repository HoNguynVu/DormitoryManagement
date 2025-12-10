using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

[Index("Username", Name = "UQ__Accounts__536C85E48235F68E", IsUnique = true)]
[Index("Email", Name = "UQ__Accounts__A9D1053432E0C4BA", IsUnique = true)]
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
    public virtual ICollection<BuildingManager> BuildingManagers { get; set; } = new List<BuildingManager>();

    [InverseProperty("User")]
    public virtual ICollection<OtpCode> OtpCodes { get; set; } = new List<OtpCode>();

    [InverseProperty("User")]
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    [InverseProperty("User")]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
