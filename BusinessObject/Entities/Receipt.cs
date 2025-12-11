using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Entities
{
    [Table("Receipts")]
    public class Receipt
    {
        [Key]
        [StringLength(128)]
        public string ReceiptID { get; set; }

        [Required]
        [StringLength(128)]
        public string StudentID { get; set; }
        [ForeignKey("StudentID")]
        public Student Student { get; set; }

        public DateTime PrintTime { get; set; } = DateTime.Now;

        [StringLength(1000)]
        public string Content { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        [StringLength(100)]
        public string PaymentType { get; set; }

        [StringLength(128)]
        public string RelatedObjectID { get; set; }
    }
}
