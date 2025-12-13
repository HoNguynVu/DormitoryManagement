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
        Task<IEnumerable<Contract>> GetContractsByStudentId(string studentId);
        Task<int> CountContractsByRoomIdAndStatus(string roomId, string status);
        Task<Contract?> GetActiveContractByStudentId(string studentId);
        Task<bool> HasPendingRenewalRequestAsync(string studentId);
        Task<IEnumerable<Contract>> GetExpiredContractsAsync(DateOnly olderThan);
    }
}
