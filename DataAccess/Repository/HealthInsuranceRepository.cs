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
                .Where(h => h.StudentID == studentId && h.Status == "Active")
                .OrderByDescending(h => h.EndDate)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasPendingInsuranceRequestAsync(string studentId)
        {
            return await _context.Receipts
                .AnyAsync(i => i.StudentID == studentId
                               && i.PaymentType == "HealthInsurance" 
                               && i.Status == "Pending");
        }

        public async Task<HealthInsurance?> GetLatestInsuranceByStudentIdAsync(string studentId)
        {
            return await _context.HealthInsurances
                .Where(h => h.StudentID == studentId)
                .OrderByDescending(h => h.CreatedAt) // Lấy đơn mới tạo gần đây nhất
                .FirstOrDefaultAsync();
        }

        public async Task<HealthInsurance?> GetInsuranceByInsuranceId(string insuranceId)
        {
            return await _context.HealthInsurances
                .FirstOrDefaultAsync(h => h.InsuranceID == insuranceId);
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
