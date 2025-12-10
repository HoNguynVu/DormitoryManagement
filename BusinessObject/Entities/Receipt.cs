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

    [Column("ContractID")]
    [StringLength(128)]
    public string ContractId { get; set; } = null!;

    [StringLength(1000)]
    public string? Content { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = null!;

    [StringLength(100)]
    public string? PaymentType { get; set; }

    [Column("RelatedObjectID")]
    [StringLength(128)]
    public string? RelatedObjectId { get; set; }

    [ForeignKey("ContractId")]
    [InverseProperty("Receipts")]
    public virtual Contract Contract { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("Receipts")]
    public virtual Student Student { get; set; } = null!;
}
