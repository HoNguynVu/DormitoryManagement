using BusinessObject.DTOs.ReportDTOs;
using BusinessObject.Entities;
using BusinessObject.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IReceiptRepository : IGenericRepository<Receipt>
    {
        Task<Receipt?> GetReceiptByTypeAndRelatedIdAsync(string paymentType, string releatedId);
        Task<Receipt?> GetPendingRequestAsync(string releatedId);
        Task<PagedResult<Receipt>> GetReceiptsByManagerPagedAsync(string managerId, int pageIndex, int pageSize);

        Task<(Receipt?, Payment?)> GetReceiptAndDateAsync(string appTransId);
        Task<GrowthStatDto> GetRevenueGrowthStatsAsync();
        Task<IEnumerable<Receipt>> GetHistoryReceiptsAsync(string paymentType, string releatedId);
    }
}
