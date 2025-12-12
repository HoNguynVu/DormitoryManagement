using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface IMaintenanceUow : ITransactionManager
    {
        public IMaintenanceRepository Maintenances { get; }
        public IRoomRepository Rooms { get; }
        public IStudentRepository Students { get; }

        public IContractRepository Contracts { get; }
        public IReceiptRepository Receipts { get; }
    }
}
