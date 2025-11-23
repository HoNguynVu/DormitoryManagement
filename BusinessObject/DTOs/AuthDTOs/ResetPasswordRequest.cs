using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.AuthDTOs
{
    public class ResetPasswordRequest
    {
        public string email { get; set; } = null!;
        public string password { get; set; } = null!;
    }
}
