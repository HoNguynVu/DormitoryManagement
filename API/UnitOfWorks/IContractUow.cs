using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface IContractUow : ITransactionManager
    {
        public IContractRepository Contracts { get; }
        public IStudentRepository Students { get; }
        public IRoomRepository Rooms { get; }
        public IViolationRepository Violations { get; }
        public IReceiptRepository Receipts { get; }
    }
}
