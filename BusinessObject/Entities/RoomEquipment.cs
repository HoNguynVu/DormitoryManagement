using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    [Table("RoomEquipments")]
    public class RoomEquipment
    {
        [Key]
        [StringLength(128)]
        public string RoomEquipmentID { get; set; } 

        [Required]
        [StringLength(128)]
        public string RoomID { get; set; }
        
        [ForeignKey("RoomID")]
        public virtual Room Room { get; set; }

        [Required]
        [StringLength(128)]
        public string EquipmentID { get; set; }
        
        [ForeignKey("EquipmentID")]
        public virtual Equipment Equipment { get; set; }

        
        [Range(1, 1000, ErrorMessage = "Số lượng phải từ 1 trở lên")]
        public int Quantity { get; set; } 

        [Required]
        [StringLength(50)]
        public string Status { get; set; } 
    }
}
