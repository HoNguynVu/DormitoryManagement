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
            return await _dbSet.FirstOrDefaultAsync(r => r.Status=="Pending" && r.RelatedObjectID == releatedId);
        }

        public async Task<PagedResult<Receipt>> GetReceiptsByManagerPagedAsync(string managerId, int pageIndex, int pageSize)
        {

            var query = _dbSet.AsNoTracking()
                .Include(r => r.Student).ThenInclude(s => s.Contracts).ThenInclude(c => c.Room)
                .Where(r => r.Student.Contracts.Any(c =>
                    c.Room.Building.ManagerID == managerId &&
                    c.ContractStatus == "Active")); 

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.PrintTime) 
                .Skip((pageIndex - 1) * pageSize)    
                .Take(pageSize)                      
                .ToListAsync();

            return new PagedResult<Receipt>(items, totalCount, pageIndex, pageSize);
        }
    }
}
