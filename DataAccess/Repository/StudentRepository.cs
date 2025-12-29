using BusinessObject.DTOs.ReportDTOs;
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
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        public StudentRepository(DormitoryDbContext context) : base(context)
        {
        }

        public async Task<Student?> GetStudentByEmailAsync(string email)
        {
            return await _dbSet
                .Include(s => s.School)
                .Include(s => s.Priority)
                .Include(s => s.Account)
                .FirstOrDefaultAsync(s => s.Email == email);
        }

        // ✅ Override để thêm eager loading
        public override async Task<Student?> GetByIdAsync(string id)
        {
            return await _dbSet
                .Include(s => s.Relatives)
                .Include(s => s.School)
                .Include(s => s.Priority)
                .Include(s => s.Account)
                .FirstOrDefaultAsync(s => s.StudentID == id);
        }
        public async Task<Student?> GetStudentByAccountIdAsync(string accountId)
        {
            return await _dbSet
                .Include(s => s.Relatives)
                .Include(s => s.School)
                .Include(s => s.Priority)
                .Include(s => s.Account)
                .FirstOrDefaultAsync(s => s.AccountID == accountId);
        }
        public async Task<IEnumerable<Student>> GetStudentsWithPriorityAsync(string? priorityId)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(s => s.Priority)   
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(priorityId))
            {
                query = query.Where(s => s.PriorityID == priorityId);
            }
            else
            {
                query = query.Where(s => s.PriorityID != null && s.PriorityID != "");
            }

            return await query.ToListAsync();
        }

        public async Task<int> CountStudentByManagerIdAsync(string managerId)
        {
            var count = await _dbSet
                .CountAsync(s => s.Contracts.Any(c =>
                    c.Room.Building.ManagerID == managerId &&
                    (c.ContractStatus == "Active" || c.ContractStatus == "NearExpiration")));

            return count;
        }

        public async Task<GrowthStatDto> GetStudentGrowthStatsAsync()
        {
            var now = DateOnly.FromDateTime(DateTime.Now);

            var startOfThisMonth = DateOnly.FromDateTime(new DateTime(now.Year, now.Month, 1));

            // 1. TÍNH SỐ LƯỢNG HIỆN TẠI (Current)

            var currentActiveContracts = await _context.Contracts
                .CountAsync(c => c.ContractStatus == "Active"
                              && c.StartDate <= now
                              && c.EndDate >= now);

            // 2. TÍNH SỐ LƯỢNG THÁNG TRƯỚC (Previous - Snapshot)

            var previousActiveContracts = await _context.Contracts
                .CountAsync(c => c.ContractStatus== "Active" // Hoặc check trạng thái tại thời điểm đó nếu có log history
                              && c.StartDate < startOfThisMonth
                              && c.EndDate >= startOfThisMonth);

            // 3. Trả về kết quả kèm % tăng trưởng
            return CalculateGrowth(currentActiveContracts, previousActiveContracts);
        }

        

        // --- Helper tính % tăng trưởng ---
        private GrowthStatDto CalculateGrowth(int current, int previous)
        {
            double growth = 0;

            // Trường hợp tháng trước có dữ liệu
            if (previous > 0)
            {
                growth = (double)(current - previous) / previous * 100;
            }
            // Trường hợp tháng trước = 0, tháng này > 0 -> Tăng 100%
            else if (current > 0)
            {
                growth = 100;
            }

            return new GrowthStatDto
            {
                TotalValue = current,
                GrowthPercent = Math.Round(growth, 1), // Làm tròn 1 số thập phân (VD: 12.5)
                IsIncrease = growth >= 0
            };
        }
    }
}
