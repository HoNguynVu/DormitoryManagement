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
    public class HealthInsuranceRepository : GenericRepository<HealthInsurance>, IHealthInsuranceRepository
    {
        public HealthInsuranceRepository(DormitoryDbContext context) : base(context)
        {
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
            return await _context.HealthInsurances
                .AnyAsync(i => i.StudentID == studentId
                               && i.Status == "Pending");
        }

        public async Task<HealthInsurance?> GetLatestInsuranceByStudentIdAsync(string studentId)
        {
            return await _context.HealthInsurances
                .Include (h => h.Student)
                .Include (h => h.Hospital)
                .Where(h => h.StudentID == studentId)
                .OrderByDescending(h => h.CreatedAt) // Lấy đơn mới tạo gần đây nhất
                .FirstOrDefaultAsync();
        }

        public async Task<HealthInsurance?> GetDetailInsuranceByIdAsync(string insuranceId)
        {
            return await _dbSet
                .Include(h=>h.Hospital)
                .Include(h=>h.Student)
                .Include(h=>h.HealthInsurancePrice)
                .AsNoTracking()
                .FirstOrDefaultAsync(h=>h.InsuranceID == insuranceId);
        }

        public async Task<IEnumerable<Hospital>> GetAllHospitalAsync()
        {
            return await _context.Hospitals.ToListAsync();
        }

        public async Task<IEnumerable<HealthInsurance>> GetHealthInsuranceFiltered(string? keyword, string? hospitalName, int? year,string? status)
        {
             var query = _context.HealthInsurances
                .Include(h => h.Student)
                .Include(h => h.Hospital)
                .Include(h => h.HealthInsurancePrice)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string key = keyword.Trim().ToLower();
                query = query.Where(h =>
                    h.Student.StudentID.Contains(key) ||
                    h.Student.FullName != null && h.Student.FullName.ToLower().Contains(key) ||
                    h.CardNumber.Contains(key)
                );
            }

            if (!string.IsNullOrEmpty(hospitalName))
            {
                query = query.Where(h=>h.Hospital.HospitalName==hospitalName);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(h=>h.Status == status);
            }

            if (year != null)
            {
                query = query.Where( h=> h.StartDate.Year == year);
            }
            return await query
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

    }
}
