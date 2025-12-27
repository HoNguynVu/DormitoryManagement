using BusinessObject.DTOs.ContractDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.StudentDTOs
{
    public class GetDashboardStudent
    {
        public ContractDetailByStudentDto? CurrentContract { get; set; }
        public int CountUnpaidBills { get; set; }
        public int CountViolations { get; set; }
        public string InsuranceStatus { get; set; } = string.Empty;
        public DateOnly? InsuranceEndDate { get; set; }
    }
}
