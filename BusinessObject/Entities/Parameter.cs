using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities
{
    [Table("Parameters")]
    public class Parameter
    {
        [Key]
        public int ParameterID { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DefaultElectricityPrice { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DefaultWaterPrice { get; set; }
        public DateTime EffectiveDate { get; set; }   // Ngày bắt đầu áp dụng
        public bool IsActive { get; set; }
    }
}
