using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities
{
    [Table("RoomTypes")]
    public class RoomType
    {
        [Key]
        [StringLength(128)]
        public string RoomTypeID { get; set; }

        [Required]
        [StringLength(100)]
        public string TypeName { get; set; }

        public int Capacity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public ICollection<Room> Rooms { get; set; }
    }
}
