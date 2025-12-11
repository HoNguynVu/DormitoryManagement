using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Entities
{
    [Table("Priorities")]
    public class Priority
    {
        [Key]
        [StringLength(128)]
        public string PriorityID { get; set; }

        [StringLength(255)]
        public string PriorityDescription { get; set; }

        public ICollection<Student> Students { get; set; }
    }
}
