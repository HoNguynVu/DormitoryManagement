using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities;

public partial class Parameter
{
    [Key]
    [Column("ParameterID")]
    public int ParameterId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal DefaultElectricityPrice { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal DefaultWaterPrice { get; set; }
}
