using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Entities
{
    [Table("OtpCodes")]
    public class OtpCode
    {
        [Key]
        [StringLength(128)]
        public string OtpID { get; set; }

        [Required]
        [StringLength(128)]
        [Column("AccountID")]
        public string AccountID { get; set; }
        [ForeignKey("AccountID")]
        public Account Account { get; set; }

        [Required]
        [StringLength(10)]
        public string Code { get; set; }

        public DateTime ExpiresAt { get; set; }

        [Required]
        [StringLength(50)]
        public string Purpose { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }
}
