using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface IBuildingUow : ITransactionManager
    {
        public IBuildingManagerRepository BuildingManagers { get; }
        public IRoomRepository Rooms { get; }
        public IBuildingRepository Buildings { get; }
        public IUtilityBillRepository UtilityBills { get; }
        public IContractRepository Contracts { get; }
        public IViolationRepository Violations { get; }
        public IStudentRepository Students { get; }
        public IMaintenanceRepository Maintenances { get; }
        public IReceiptRepository Receipts { get; }
        public IAccountRepository Accounts { get; }
    }
}
