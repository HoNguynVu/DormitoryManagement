using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

[Index("SchoolName", IsUnique = true)]
public partial class School
{
    [Key]
    [Column("SchoolID")]
    [StringLength(128)]
    public string SchoolId { get; set; } = null!;

    [StringLength(255)]
    public string SchoolName { get; set; } = null!;

    [StringLength(500)]
    public string? Address { get; set; }

    [InverseProperty("School")]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
