using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Entities
{
    [Table("RefreshTokens")]
    public class RefreshToken
    {
        [Key]
        [StringLength(128)]
        public string TokenID { get; set; }

        [Required]
        [StringLength(128)]
        public string AccountID { get; set; }
        [ForeignKey("AccountID")]
        public Account Account { get; set; }

        [Required]
        [StringLength(450)]
        public string Token { get; set; }

        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? RevokedAt { get; set; }
    }
}
