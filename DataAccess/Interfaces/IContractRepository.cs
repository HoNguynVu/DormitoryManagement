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
        Task<Contract?> GetActiveContractByStudentId(string studentId);
        Task<Contract?> GetContractById(string contractId);
        void UpdateContract(Contract contract);
    }
}
