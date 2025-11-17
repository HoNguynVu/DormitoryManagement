using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

public partial class Priority
{
    [Key]
    [Column("PriorityID")]
    [StringLength(128)]
    public string PriorityId { get; set; } = null!;

    [StringLength(255)]
    public string? PriorityDescription { get; set; }

    [InverseProperty("Priority")]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
