using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    [Table("MaintenanceRequests")]
    public class MaintenanceRequest
    {
        [Key]
        [StringLength(128)]
        public string RequestID { get; set; }

        [Required]
        public string StudentID { get; set; }
        [ForeignKey("StudentID")]
        public virtual Student Student { get; set; }

        [Required]
        public string RoomID { get; set; } // Sự cố xảy ra ở phòng nào
        [ForeignKey("RoomID")]
        public virtual Room Room { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } // Mô tả hư hỏng

        public DateTime RequestDate { get; set; } = DateTime.Now;
        public DateTime? ResolvedDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Cancelled

        [Column(TypeName = "decimal(18, 2)")]
        public decimal RepairCost { get; set; } = 0;

        // Ghi chú của quản lý (VD: "Đã thay bóng đèn", "Sinh viên làm vỡ")
        public string? ManagerNote { get; set; }
    }
}
