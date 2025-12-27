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
using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class BuildingRepository : GenericRepository<Building>, IBuildingRepository
    {
        public BuildingRepository(DormitoryDbContext context) : base(context)
        {
        }

        public async Task<List<BuildingPerformanceDto>> GetBuildingPerformanceAsync()
        {
            var now = DateTime.Now;
            var startOfThisMonth = new DateTime(now.Year, now.Month, 1);

            var query = _context.Buildings
                .Select(b => new
                {
                    b.BuildingID,
                    b.BuildingName,
                    TotalBeds = b.Rooms.Sum(r => r.Capacity),

                    // Tính số giường đang sử dụng 
                    UsedBeds = b.Rooms.SelectMany(r => r.Contracts)
                                      .Count(c => c.ContractStatus == "Active"
                                               && c.StartDate <= DateOnly.FromDateTime(now)
                                               && c.EndDate >= DateOnly.FromDateTime(now)),

                    // Tính doanh thu tháng này của Tòa
                    MonthlyRevenue = b.Rooms
                        .SelectMany(r => r.Contracts)
                        .Where(c => c.ContractStatus == "Active")
                        .Select(c => c.Student)
                        .SelectMany(s => s.Receipts)
                        .Where(i => i.Status == "Success" && i.PrintTime >= startOfThisMonth)
                        .Sum(rec => (decimal?)rec.Amount) ?? 0
                });
            var data = await query.ToListAsync();

            return data.Select(x => new BuildingPerformanceDto
            {
                BuildingId = x.BuildingID,
                BuildingName = x.BuildingName,
                TotalBeds = x.TotalBeds,
                UsedBeds = x.UsedBeds,
                MonthlyRevenue = x.MonthlyRevenue,

                OccupancyRate = x.TotalBeds > 0
                    ? Math.Round((double)x.UsedBeds / x.TotalBeds * 100, 3)
                    : 0
            }).ToList();
        }
        public async Task<Building?> GetByManagerId(string managerId)
        {
            return await _dbSet.Where(b => b.ManagerID == managerId).FirstOrDefaultAsync();
        }
    }
}
