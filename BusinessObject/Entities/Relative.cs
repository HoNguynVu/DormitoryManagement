using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Base;

namespace BusinessObject.Entities
{
    [Table("Relatives")]
    public class Relative : Person
    {
        [Key]
        [StringLength(128)]
        public string RelativeID { get; set; }

        [Required]
        [StringLength(128)]
        public string StudentID { get; set; }
        [ForeignKey("StudentID")]
        public Student Student { get; set; }

        [StringLength(100)]
        public string Occupation { get; set; }

        
    }
}
