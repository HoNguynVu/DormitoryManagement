using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface IContractUow : ITransactionManager
    {
        public IContractRepository Contracts { get; }
        public IStudentRepository Students { get; }
        public IRoomRepository Rooms { get; }

        public IBuildingRepository Buildings { get; }
        public IViolationRepository Violations { get; }
        public IReceiptRepository Receipts { get; }
        public IPaymentRepository Payments { get; }
        public IAccountRepository Accounts { get; }
        public INotificationRepository Notifications { get; }
        public IBuildingManagerRepository BuildingManagers { get; }
        public IRoomEquipmentRepository RoomEquipments { get; }
    }
}
