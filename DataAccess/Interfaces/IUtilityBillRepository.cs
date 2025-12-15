using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;

namespace DataAccess.Interfaces
{
    public interface IUtilityBillRepository : IGenericRepository<UtilityBill>
    {
        Task<UtilityBill?> GetByRoomAndPeriodAsync(string roomId, int month, int year);
        Task<UtilityBill?> GetLastMonthBillAsync(string roomId);
        Task<bool> IsBillExistsAsync (string roomId, int month, int year);
        Task<IEnumerable<UtilityBill>> GetByRoomAsync(string roomId);
    }
}
