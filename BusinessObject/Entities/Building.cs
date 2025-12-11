using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Entities
{
    [Table("Buildings")]
    public class Building
    {
        [Key]
        [Column("BuildingID")]
        [StringLength(128)]
        public string BuildingID { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string BuildingName { get; set; } = null!;

        [Column("ManagerID")]
        [StringLength(128)]
        public string ManagerID { get; set; } = null!;
        [ForeignKey("ManagerID")]
        public BuildingManager Manager { get; set; } = null!;

        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
