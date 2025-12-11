using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.ContractDTOs
{
    public class RenewalRequestDto
    {
        public string StudentId { get; set; } = string.Empty;
        public int MonthsToExtend { get; set; }
    }
}
