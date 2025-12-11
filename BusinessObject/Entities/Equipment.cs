using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities
{
    [Table("Equipment")] // Lưu ý: Tên bảng trong SQL là số ít
    public class Equipment
    {
        [Key]
        [StringLength(128)]
        public string EquipmentID { get; set; }

        [Required]
        [StringLength(128)]
        public string RoomID { get; set; }
        [ForeignKey("RoomID")]
        public Room Room { get; set; }

        [Required]
        [StringLength(100)]
        public string EquipmentName { get; set; }

        [StringLength(100)]
        public string Status { get; set; }
    }
}
