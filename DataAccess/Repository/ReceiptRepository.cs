using BusinessObject.DTOs.ReportDTOs;
using BusinessObject.Entities;
using BusinessObject.Helpers;
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
    public class ReceiptRepository : GenericRepository<Receipt>, IReceiptRepository
    {
        public ReceiptRepository(DormitoryDbContext context) : base(context)
        {
        }
        public async Task<Receipt?> GetReceiptByTypeAndRelatedIdAsync(string paymentType, string relatedId)
        {
            return await _dbSet.FirstOrDefaultAsync(r => r.PaymentType == paymentType && r.RelatedObjectID == relatedId);
        }

        public async Task<Receipt?> GetPendingRequestAsync(string releatedId)
        {
            return await _dbSet.FirstOrDefaultAsync(r => r.Status == "Pending" && r.RelatedObjectID == releatedId);
        }

        public async Task<PagedResult<Receipt>> GetReceiptsByManagerPagedAsync(string managerId, int pageIndex, int pageSize)
        {

            var query = _dbSet.AsNoTracking()
                .Include(r => r.Student).ThenInclude(s => s.Contracts).ThenInclude(c => c.Room)
                .Where(r => r.Student.Contracts.Any(c =>
                    c.Room.Building.ManagerID == managerId &&
                    c.ContractStatus == "Active" || c.ContractStatus == "NearExpiration"));

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.PrintTime)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Receipt>(items, totalCount, pageIndex, pageSize);
        }

        public async Task<GrowthStatDto> GetRevenueGrowthStatsAsync()
        {
            var now = DateOnly.FromDateTime(DateTime.Now);
            var startOfThisMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfThisMonth.AddMonths(-1);

            // 1. Doanh thu tháng này (Từ ngày 1 -> Hôm nay)
            var currentRevenue = await _context.Receipts
                .Where(r => r.Status == "Success" && r.PrintTime >= startOfThisMonth)
                .SumAsync(r => (decimal?)r.Amount) ?? 0; // Handle null nếu chưa có đơn nào

            // 2. Doanh thu tháng trước (Trọn vẹn tháng trước)
            // Logic: Từ đầu tháng trước -> TRƯỚC đầu tháng này
            var lastMonthRevenue = await _context.Receipts
                .Where(r => r.Status == "Success"
                         && r.PrintTime >= startOfLastMonth
                         && r.PrintTime < startOfThisMonth)
                .SumAsync(r => (decimal?)r.Amount) ?? 0;

            // 3. Tính % tăng trưởng (Dùng hàm helper bên dưới)
            return CalculateGrowthDecimal(currentRevenue, lastMonthRevenue);
        }

        // --- Helper tính % cho số tiền (Decimal) ---
        private GrowthStatDto CalculateGrowthDecimal(decimal current, decimal previous)
        {
            double growth = 0;

            if (previous > 0)
            {
                // Ép kiểu double để chia ra số thập phân
                growth = (double)((current - previous) / previous) * 100;
            }
            else if (current > 0)
            {
                growth = 100; 
            }

            return new GrowthStatDto
            {
                TotalValue = (int)current,
                GrowthPercent = Math.Round(growth, 1),
                TotalMoney = current,
                IsIncrease = growth >= 0
            };
        }
    }
}
