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
                                && i.PaymentType == "Renewal"
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

        public async Task<IEnumerable<Contract>> GetExpiringContractsByManagerIdAsync(DateOnly beforeDate, string managerId)
        {
            return await _dbSet
                .Include(c => c.Student)
                .Include(c => c.Room).ThenInclude(r => r.Building)
                .Include(c => c.Room).ThenInclude(r => r.RoomType)
                .Where(c => c.EndDate != null 
                            && c.EndDate <= beforeDate 
                            && c.ContractStatus == "Active"
                            && c.Room.Building.ManagerID == managerId)
                .ToListAsync();
        }

    }
}
