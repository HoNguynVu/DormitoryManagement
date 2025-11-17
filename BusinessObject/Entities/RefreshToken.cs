using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

[Index("Token", Name = "UQ__RefreshT__1EB4F817B9CC7893", IsUnique = true)]
public partial class RefreshToken
{
    [Key]
    [Column("TokenID")]
    [StringLength(128)]
    public string TokenId { get; set; } = null!;

    [Column("UserID")]
    [StringLength(128)]
    public string UserId { get; set; } = null!;

    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("RefreshTokens")]
    public virtual Account User { get; set; } = null!;
}
