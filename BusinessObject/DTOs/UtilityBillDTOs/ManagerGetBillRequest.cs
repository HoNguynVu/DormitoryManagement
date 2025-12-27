using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.UtilityBillDTOs
{
    public class ManagerGetBillRequest
    {
        public string AccountId { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
