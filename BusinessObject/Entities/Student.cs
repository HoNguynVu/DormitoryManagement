using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObject.Base;

namespace BusinessObject.Entities
{
    [Table("Students")]
    public class Student : Person
    {
        [Key]
        [StringLength(128)]
        public string StudentID { get; set; }

        [StringLength(128)]
        public string AccountID { get; set; }
        [ForeignKey("AccountID")]
        public Account Account { get; set; }

        [Required]
        [StringLength(20)]
        public string CitizenID { get; set; }

        [StringLength(255)]
        public string CitizenIDIssuePlace { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(128)]
        public string SchoolID { get; set; }
        [ForeignKey("SchoolID")]
        public School School { get; set; }

        [StringLength(128)]
        public string PriorityID { get; set; }
        public string Gender { get; set; }
        [ForeignKey("PriorityID")]
        public Priority Priority { get; set; }

        // Navigation Properties
        public ICollection<Relative> Relatives { get; set; }
        public ICollection<Contract> Contracts { get; set; }
        public ICollection<RegistrationForm> RegistrationForms { get; set; }
        public ICollection<Violation> Violations { get; set; }
        public ICollection<Receipt> Receipts { get; set; }
        public ICollection<HealthInsurance> HealthInsurances { get; set; }
    }
}