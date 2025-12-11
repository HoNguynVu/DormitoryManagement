using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Entities
{
    [Table("Violations")]
    public class Violation
    {
        [Key]
        [StringLength(128)]
        public string ViolationID { get; set; }

        [Required]
        [StringLength(128)]
        public string StudentID { get; set; }
        [ForeignKey("StudentID")]
        public Student Student { get; set; }

        [StringLength(128)]
        public string ReportingManagerID { get; set; }
        [ForeignKey("ReportingManagerID")]
        public BuildingManager ReportingManager { get; set; }

        [Required]
        [StringLength(255)]
        public string ViolationAct { get; set; }

        public DateTime ViolationTime { get; set; } = DateTime.Now;

        [StringLength(1000)]
        public string Description { get; set; }

        [StringLength(255)]
        public string Resolution { get; set; }
    }
}
