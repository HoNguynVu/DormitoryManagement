using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface IContractUow : ITransactionManager
    {
        public IContractRenewalRepository ContractRenewals { get; }
        public IContractRepository Contracts { get; }
    }
}
