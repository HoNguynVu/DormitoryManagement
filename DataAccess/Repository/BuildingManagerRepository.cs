using BusinessObject.DTOs.ReportDTOs;
using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class BuildingManagerRepository : GenericRepository<BuildingManager>, IBuildingManagerRepository
    {
        public BuildingManagerRepository(DormitoryDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<BuildingManager>> GetAllWithBuildingsAsync()
        {
            return await _dbSet
                .Include(bm => bm.Buildings)
                .ToListAsync();
        }

        // Ensure single manager load includes Buildings
        public override async Task<BuildingManager?> GetByIdAsync(string id)
        {
            return await _dbSet
                .Include(bm => bm.Buildings)
                .FirstOrDefaultAsync(bm => bm.ManagerID == id);
        }

        public async Task<BuildingManager?> GetByAccountIdAsync(string accountId)
        {
            return await _dbSet
                .Include(bm => bm.Buildings)
                .FirstOrDefaultAsync(bm => bm.AccountID == accountId);
        }
        public async Task<GrowthStatDto> GetStaffGrowthStatsAsync()
        {
            var now = DateTime.Now;
            var startOfThisMonth = new DateTime(now.Year, now.Month, 1);

            // 1. S? l??ng hi?n t?i
            // Logic: ??m nh?ng User có vai trò là Manager ho?c Staff và ?ang Active
            // N?u b?n ch? mu?n ??m Manager: b? "Staff" ?i
            var currentStaff = await _context.Accounts
                .CountAsync(u => (u.Role == "Manager")
                              && u.IsActive == true);

            // 2. S? l??ng tháng tr??c (Snapshot ??u tháng)
            // Logic: Nh?ng nhân viên ?ã ???c t?o tài kho?n tr??c ngày 1 tháng này
            var previousStaff = await _context.Accounts
                .CountAsync(u => (u.Role == "Manager" || u.Role == "Staff")
                              && u.IsActive == true
                              && u.CreatedAt < startOfThisMonth);

            // 3. Tính % t?ng tr??ng
            return CalculateGrowth(currentStaff, previousStaff);
        }

        // --- Helper tính % (Gi? nguyên logic c?) ---
        private GrowthStatDto CalculateGrowth(int current, int previous)
        {
            double growth = 0;
            if (previous > 0)
            {
                growth = (double)(current - previous) / previous * 100;
            }
            else if (current > 0)
            {
                growth = 100; // T? 0 lên có ng??i -> T?ng 100%
            }

            return new GrowthStatDto
            {
                TotalValue = current,
                GrowthPercent = Math.Round(growth, 1),
                IsIncrease = growth >= 0
            };
        }
    }
}
