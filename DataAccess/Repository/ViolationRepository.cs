using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class ViolationRepository : IViolationRepository
    {
        private readonly DormitoryDbContext _context;

        public ViolationRepository(DormitoryDbContext context)
        {
            _context = context;
        }

        public async Task<Violation?> GetViolationById(string violationId)
        {
            return await _context.Violations
                .Include(v => v.Student)
                .Include(v => v.ReportingManager)
                .FirstOrDefaultAsync(v => v.ViolationId == violationId);
        }

        public async Task<IEnumerable<Violation>> GetViolationsByStudentId(string studentId)
        {
            return await _context.Violations
                .Include(v => v.Student)
                .Include(v => v.ReportingManager)
                .Where(v => v.StudentId == studentId)
                .OrderByDescending(v => v.ViolationTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Violation>> GetAllViolations()
        {
            return await _context.Violations
                .Include(v => v.Student)
                .Include(v => v.ReportingManager)
                .OrderByDescending(v => v.ViolationTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Violation>> GetPendingViolations()
        {
            return await _context.Violations
                .Include(v => v.Student)
                .Include(v => v.ReportingManager)
                .Where(v => string.IsNullOrEmpty(v.Resolution))
                .OrderByDescending(v => v.ViolationTime)
                .ToListAsync();
        }

        public async Task<int> CountViolationsByStudentId(string studentId)
        {
            return await _context.Violations
                .Where(v => v.StudentId == studentId)
                .CountAsync();
        }

        public void AddViolation(Violation violation)
        {
            _context.Violations.Add(violation);
        }

        public void UpdateViolation(Violation violation)
        {
            _context.Violations.Update(violation);
        }

        public void DeleteViolation(Violation violation)
        {
            _context.Violations.Remove(violation);
        }
    }
}