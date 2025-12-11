using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class HealthInsuranceRepository : IHealthInsuranceRepository
    {
        private readonly DormitoryDbContext _context;

        public HealthInsuranceRepository(DormitoryDbContext context)
        {
            _context = context;
        }

        public async Task<HealthInsurance?> GetActiveInsuranceByStudentIdAsync(string studentId)
        {
            return await _context.HealthInsurances
                .Include(h => h.Student)
                .Where(h => h.StudentId == studentId && h.Status == "Active")
                .OrderByDescending(h => h.EndDate)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasPendingInsuranceRequestAsync(string studentId)
        {
            return await _context.Receipts
                .AnyAsync(i => i.StudentId == studentId
                               && i.PaymentType == "HealthInsurance" 
                               && i.Status == "Pending");
        }

        public void Add(HealthInsurance insurance)
        {
            _context.HealthInsurances.Add(insurance);
        }

        public void Update(HealthInsurance insurance)
        {
            _context.HealthInsurances.Update(insurance);
        }
    }
}
