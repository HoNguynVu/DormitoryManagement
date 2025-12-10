using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.RegisDTOs
{
    public class RegistrationFormRequest
    {
        public string StudentId { get; set; } = null!;
        public string RoomId { get; set; } = null!;
    }
}
