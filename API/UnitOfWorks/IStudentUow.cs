using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface IStudentUow : ITransactionManager
    {
        public IStudentRepository Students { get; }
        public IRelativeRepository Relatives { get; }
        public IContractRepository Contracts { get; }
        public IHealthInsuranceRepository HealthInsurances { get; }
        public IUtilityBillRepository UtilityBills { get; }
        public IViolationRepository Violations { get; }

        public INotificationRepository Notifications { get; }
    }
}
