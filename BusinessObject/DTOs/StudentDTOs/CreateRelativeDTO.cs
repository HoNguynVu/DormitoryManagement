using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.StudentDTOs
{
    public class CreateRelativeDTO
    {
        public string FullName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Occupation { get; set; } = string.Empty;
        public string StudentID { get; set; } = string.Empty;
    }
}
