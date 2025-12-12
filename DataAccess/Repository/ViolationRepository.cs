using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class ViolationRepository : GenericRepository<Violation>, IViolationRepository
    {
        public ViolationRepository(DormitoryDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Violation>> GetViolationsByStudentId(string studentId)
        {
            return await _dbSet
                .Include(v => v.Student)
                .Include(v => v.ReportingManager)
                .Where(v => v.StudentID == studentId)
                .OrderByDescending(v => v.ViolationTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Violation>> GetPendingViolations()
        {
            return await _dbSet
                .Include(v => v.Student)
                .Include(v => v.ReportingManager)
                .Where(v => string.IsNullOrEmpty(v.Resolution))
                .OrderByDescending(v => v.ViolationTime)
                .ToListAsync();
        }

        public async Task<int> CountViolationsByStudentId(string studentId)
        {
            return await _dbSet.CountAsync(v => v.StudentID == studentId);
        }

        // ✅ Override để thêm eager loading
        public override async Task<Violation?> GetByIdAsync(string id)
        {
            return await _dbSet
                .Include(v => v.Student)
                .Include(v => v.ReportingManager)
                .FirstOrDefaultAsync(v => v.ViolationID == id);
        }
    }
}