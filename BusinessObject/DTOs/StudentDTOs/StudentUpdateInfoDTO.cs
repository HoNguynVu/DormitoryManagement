using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.StudentDTOs
{
    public class StudentUpdateInfoDTO
    {
        public string StudentID { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string SchoolID { get; set; } = string.Empty;
        public string PriorityID { get; set; } = string.Empty;
        public string CitizenIDIssuePlace { get; set; } = string.Empty;

    }
}
