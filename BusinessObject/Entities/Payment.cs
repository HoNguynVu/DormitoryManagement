using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Entities
{
    [Table("Payments")]
    public class Payment
    {
        [Key]
        [StringLength(128)]
        public string PaymentID { get; set; }

        [Required]
        [StringLength(128)]
        public string ReceiptID { get; set; }
        // Lưu ý: SQL không có FK constraint cho ReceiptID trong bảng Payments, nhưng logic thì có
        
        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string TransactionID { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }
    }
}
