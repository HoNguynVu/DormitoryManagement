using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        public string NotificationID { get; set; } = null!;

        [Required]
        [StringLength(128)]
        public string AccountID { get; set; } = null!;

        [ForeignKey("AccountID")]
        public Account? Account { get; set; }  // ✅ Thêm navigation property

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false; // Đã xem chưa
        public string Type { get; set; } // "Bill", "Violation", "System"
    }
}
