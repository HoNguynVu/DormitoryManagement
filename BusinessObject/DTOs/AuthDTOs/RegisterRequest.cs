using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.AuthDTOs
{
    public class RegisterRequest
    {
        public string StudentId { get; set; }
        public string FullName { get; set; }
        public string CitizenId { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string SchoolId { get; set; }
        public string PriorityId { get; set; }
        public string Password { get; set; }        
    }
}
