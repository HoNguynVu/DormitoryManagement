using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.AuthDTOs
{
    public class ForgotPasswordRequest
    {
        public string email { get; set; } = null!;
    }
}
