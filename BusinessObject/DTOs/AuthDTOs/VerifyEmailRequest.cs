using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.AuthDTOs
{
    public class VerifyEmailRequest
    {
        public string OTP { get; set; }
        public string Email { get; set; }
    }
}
