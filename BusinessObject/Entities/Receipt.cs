using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

public partial class Receipt
{
    [Key]
    [Column("ReceiptID")]
    [StringLength(128)]
    public string ReceiptId { get; set; } = null!;

    [Column("StudentID")]
    [StringLength(128)]
    public string StudentId { get; set; } = null!;

    public DateTime PrintTime { get; set; }

    [StringLength(1000)]
    public string? Content { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = null!;

    [StringLength(100)]
    public string PaymentType { get; set; } = null!;

    [Column("RelatedObjectID")]
    [StringLength(128)]
    public string RelatedObjectId { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("Receipts")]
    public virtual Student Student { get; set; } = null!;
}
