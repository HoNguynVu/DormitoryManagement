using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;

namespace DataAccess.Interfaces
{
    public interface IContractRepository : IGenericRepository<Contract>
    {
        Task<IEnumerable<Contract>> GetContractsByRoomIdAndStatus(string roomId, string status);
        Task<IEnumerable<Contract>> GetContractsByStudentId(string studentId);
        Task<int> CountContractsByRoomIdAndStatus(string roomId, string status);
        Task<Contract?> GetActiveContractByStudentId(string studentId);
        Task<Contract?> GetActiveAndNearExpiringContractByStudentId(string studentId);
        Task<bool> HasPendingRenewalRequestAsync(string studentId);
        Task<IEnumerable<Contract>> GetExpiredContractsAsync(DateOnly olderThan);
        Task<IEnumerable<Contract>> GetExpiringContractsByManagerIdAsync(DateOnly fromDate, DateOnly beforeDate, string managerId);
        Task<int> CountExpiringContractsByManagerIdAsync(DateOnly fromDate, DateOnly beforeDate, string managerId);

        Task<IEnumerable<Contract>> GetContractsFilteredAsync(string? keyword, string? buildingId, string? status, DateOnly? startDate,DateOnly? endDate);
        Task<Contract?> GetDetailContractAsync(string contractId);
        Task<Dictionary<string, int>> CountContractsByStatusAsync(string? buildingId = null);
        Task<Contract?> GetLastContractByStudentIdAsync(string studentId);
        Task<Dictionary<string, int>> CountActiveContractsByRoomAsync();
        Task<int> CountWarningContractsAsync(int daysThreshold);
    }
}
