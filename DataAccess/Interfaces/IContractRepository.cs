using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;

namespace DataAccess.Interfaces
{
    public interface IContractRepository
    {
        Task<IEnumerable<Contract>> GetAllContracts();
        Task<Contract?> GetContractById(string contractId);
        Task<IEnumerable<Contract>> GetContractsByStudentId(string studentId);
        Task<Contract?> GetActiveContractByStudentId(string studentId);
        Task<int> CountContractsByRoomIdAndStatus(string roomId, string status);
        void AddContract(Contract contract);
        void UpdateContract(Contract contract);
        void DeleteContract(Contract contract);
    }
}
