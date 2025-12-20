using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.ConfirmDTOs
{
    public class BaseConfirmDto
    {
        public string StudentName { get; set; } = null!;
        public string StudentEmail { get; set; } = null!;
    }
}
