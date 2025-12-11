using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Entities
{
    [Table("RegistrationForms")]
    public class RegistrationForm
    {
        [Key]
        [StringLength(128)]
        public string FormID { get; set; }

        [Required]
        [StringLength(128)]
        public string StudentID { get; set; }
        [ForeignKey("StudentID")]
        public Student Student { get; set; }

        [Required]
        [StringLength(128)]
        public string RoomID { get; set; }
        [ForeignKey("RoomID")]
        public Room Room { get; set; }

        public DateTime RegistrationTime { get; set; } = DateTime.Now;

        [Required]
        [StringLength(30)]
        public string Status { get; set; }
    }
}
