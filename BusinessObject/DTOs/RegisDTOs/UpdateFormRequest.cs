using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.RegisDTOs
{
    public class UpdateFormRequest
    {
        public string FormId { get; set; } = null!;
        public string Status { get; set; } = null!;
    }
}
