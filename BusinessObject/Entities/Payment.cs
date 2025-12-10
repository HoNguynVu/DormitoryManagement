using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

public partial class Payment
{
    [Key]
    [Column("PaymentID")]
    [StringLength(128)]
    public string PaymentId { get; set; } = null!;

    [Column("ReceiptID")]
    [StringLength(128)]
    public string ReceiptId { get; set; } = null!;

    [StringLength(50)]
    public string PaymentMethod { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    [Column("TransactionID")]
    [StringLength(100)]
    public string? TransactionId { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;
}
