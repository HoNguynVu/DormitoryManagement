using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Entities
{
    [Table("Contracts")]
    public class Contract
    {
        [Key]
        [StringLength(128)]
        public string ContractID { get; set; }

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

        [Column(TypeName = "date")]
        public DateOnly StartDate { get; set; }

        [Column(TypeName = "date")]
        public DateOnly? EndDate { get; set; }

        [Required]
        [StringLength(30)]
        public string ContractStatus { get; set; }
    }
}
