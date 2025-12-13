using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;

namespace DataAccess.Interfaces
{
    internal interface IUtilityBillRepository : IGenericRepository<UtilityBill>
    {
        Task<UtilityBill?> GetByRoomAndPeriodAsync(string roomId, int month, int year);
        Task<UtilityBill?> GetLastMonthBillAsync(string roomId);
        Task<UtilityBill?> GetByRoomAsync(string roomId);
    }
}
