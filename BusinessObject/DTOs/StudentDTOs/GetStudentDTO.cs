using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.StudentDTOs
{
    public class GetStudentDTO
    {
        public string StudentID { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string SchoolName { get; set; } = string.Empty;
        public string PriorityName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string CitizenID { get; set; } = string.Empty;
        public string CitizenIDIssuePlace { get; set; } = string.Empty;
        public List<Relative> Relatives { get; set; } = new List<Relative>();
    }
}
