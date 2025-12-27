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
    public class ContractRepository : GenericRepository<Contract>, IContractRepository
    {
        public ContractRepository(DormitoryDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Contract>> GetContractsByStudentId(string studentId)
        {
            return await _dbSet
                .Where(c => c.StudentID == studentId)
                .ToListAsync();
        }

        public async Task<int> CountContractsByRoomIdAndStatus(string roomId, string status)
        {
            return await _dbSet
                .CountAsync(c => c.RoomID == roomId && c.ContractStatus == status);
        }

        public async Task<IEnumerable<Contract>> GetContractsByRoomIdAndStatus(string roomId, string status)
        {
            return await _dbSet
                .Include(c => c.Student).ThenInclude(s => s.Account)
                .Where(c => c.RoomID == roomId && c.ContractStatus == status)
                .ToListAsync();
        }

        public async Task<Contract?> GetActiveContractByStudentId(string studentId)
        {
            return await _dbSet
                .Include(c => c.Student)
                .Include(c => c.Room)
                    .ThenInclude(r => r.RoomType)
                .Where(c => c.StudentID == studentId && 
                           (c.ContractStatus == "Active" || c.ContractStatus == "Pending"))
                .OrderByDescending(c => c.StartDate)
                .FirstOrDefaultAsync();
        }
        public async Task<bool> HasPendingRenewalRequestAsync(string studentId)
        {
            return await _context.Receipts
                 .AnyAsync(i => i.StudentID == studentId
                                && i.PaymentType == "RenewalContract"
                                && i.Status == "Pending");
        }

        public async Task<IEnumerable<Contract>> GetExpiredContractsAsync(DateOnly olderThan)
        {
            return await _dbSet
                .Include(c => c.Student)
                .Include(c => c.Room)
                    .ThenInclude(r => r.RoomType)
                .Where(c => c.EndDate != null && c.EndDate < olderThan && c.ContractStatus == "Active")
                .ToListAsync();
        }

        public async Task<IEnumerable<Contract>> GetExpiringContractsByManagerIdAsync(DateOnly fromDate, DateOnly beforeDate, string managerId)
        {
            return await _dbSet
                .Include(c => c.Student)
                .Include(c => c.Room).ThenInclude(r => r.Building)
                .Include(c => c.Room).ThenInclude(r => r.RoomType)
                .Where(c => c.EndDate != null 
                            && c.StartDate >= fromDate
                            && c.EndDate <= beforeDate 
                            && c.ContractStatus == "Active"
                            && c.Room.Building.ManagerID == managerId)
                .ToListAsync();
        }
        public async Task<int> CountExpiringContractsByManagerIdAsync(DateOnly fromDate, DateOnly beforeDate, string managerId)
        {
            return await _dbSet
                .Include(c => c.Room).ThenInclude(r => r.Building)
                .Where(c => c.EndDate != null
                            && c.StartDate >= fromDate
                            && c.EndDate <= beforeDate
                            && c.ContractStatus == "Active"
                            && c.Room.Building.ManagerID == managerId)
                .CountAsync();
        }

        public async Task<IEnumerable<Contract>> GetContractsFilteredAsync(string? keyword, string? buildingId, string? status)
        {
            // 1. Kh?i t?o Query
            var query = _context.Contracts
                .Include(c => c.Student) 
                .Include(c => c.Room).ThenInclude(r => r.Building) 
                .Include(c => c.Room).ThenInclude(r => r.RoomType) 
                .AsNoTracking()
                .AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                string key = keyword.Trim().ToLower();
                query = query.Where(c =>
                    c.StudentID.ToLower().Contains(key) ||
                    (c.Student.FullName != null && c.Student.FullName.ToLower().Contains(key)) ||
                    (c.Room.RoomName != null && c.Room.RoomName.ToLower().Contains(key))
                );
            }

            if (!string.IsNullOrEmpty(buildingId))
            {
                query = query.Where(c => c.Room.BuildingID == buildingId);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(c => c.ContractStatus == status);
            }


            return await query
                .OrderByDescending(c => c.StartDate) // M?i nh?t lên ??u
                .ToListAsync();
        }

        public async Task<Contract?> GetDetailContractAsync(string contractId)
        {
            return await _dbSet
                .Include(c => c.Student)
                .Include(c => c.Room)
                    .ThenInclude(r => r.Building)
                .Include(c => c.Room)
                    .ThenInclude(r => r.RoomType)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ContractID == contractId);
        }

        public async Task<Dictionary<string, int>> CountContractsByStatusAsync()
        {
            var result = await _dbSet
                .GroupBy(c => c.ContractStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .AsNoTracking()
                .ToListAsync();
            return result.ToDictionary(x => x.Status, x => x.Count);
        }

        public async Task<Contract?> GetLastContractByStudentIdAsync(string studentId)
        {
            return await _dbSet
                .Include(c => c.Room)
                    .ThenInclude(r => r.Building)
                    .ThenInclude(b => b.Manager)
                .Include(c => c.Room)
                    .ThenInclude(r => r.RoomType)
                .Include(c => c.Room)
                    .ThenInclude(r => r.RoomEquipments).ThenInclude(re => re.Equipment)
                .Where(c => c.StudentID == studentId)
                .OrderByDescending(c => c.StartDate)
                .FirstOrDefaultAsync();
        }
    }
}
