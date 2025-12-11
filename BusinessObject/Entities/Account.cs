using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

[Table("Accounts")]
[Index("Username", Name = "UQ__Accounts__536C85E491C65809", IsUnique = true)]
[Index("Email", Name = "UQ__Accounts__A9D10534E8EB8D7E", IsUnique = true)]
public partial class Account
{
    [Key]
    [Column("UserID")]
    [StringLength(128)]
    public string UserId { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Username { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    [Required]
    [StringLength(20)]
    public string Role { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public bool IsEmailVerified { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [InverseProperty("Account")]
    public virtual ICollection<BuildingManager> BuildingManagers { get; set; } = new List<BuildingManager>();

    [InverseProperty("Account")]
    public virtual ICollection<OtpCode> OtpCodes { get; set; } = new List<OtpCode>();

    [InverseProperty("Account")]
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    [InverseProperty("Account")]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
