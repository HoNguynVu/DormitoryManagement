using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities
{
    [Table("UtilityBills")]
    public class UtilityBill
    {
        [Key]
        [StringLength(128)]
        public string BillID { get; set; }

        [Required]
        [StringLength(128)]
        public string RoomID { get; set; }
        [ForeignKey("RoomID")]
        public Room Room { get; set; }
        public int ElectricityOldIndex { get; set; } // Chỉ số điện cũ
        public int ElectricityNewIndex { get; set; } // Chỉ số điện mới

        public int WaterOldIndex { get; set; }       // Chỉ số nước cũ
        public int WaterNewIndex { get; set; }
        public int ElectricityUsage { get; set; }
        public int WaterUsage { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }
    }
}
