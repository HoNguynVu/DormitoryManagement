using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Interfaces;
using BusinessObject.Entities;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class UtilityBillRepository  : GenericRepository<UtilityBill>, IUtilityBillRepository
    {
        public UtilityBillRepository(DormitoryDbContext context) : base(context)
        {
        }
        public async Task<bool> IsBillExistsAsync(string roomId, int month, int year)
        {
            return await _dbSet.AnyAsync(bill => bill.RoomID == roomId && bill.Month == month && bill.Year == year);
        }
        public async Task<UtilityBill?> GetByRoomAndPeriodAsync(string roomId, int month, int year)
        {
            return await _dbSet.FirstOrDefaultAsync(bill => bill.RoomID == roomId && bill.Month == month && bill.Year == year);
        }
        public async Task<UtilityBill?> GetLastMonthBillAsync(string roomId)
        {
            var currentDate = DateTime.Now;
            var lastMonth = currentDate.AddMonths(-1);
            return await _dbSet.FirstOrDefaultAsync(bill => bill.RoomID == roomId && bill.Month == lastMonth.Month && bill.Year == lastMonth.Year);
        }
        public async Task<IEnumerable<UtilityBill>> GetByRoomAsync(string roomId)
        {
            return await _dbSet.Where(bill => bill.RoomID == roomId)
                               .OrderByDescending(bill => bill.Year)
                               .ThenByDescending(bill => bill.Month)
                               .ToListAsync();
        }

    }
}
