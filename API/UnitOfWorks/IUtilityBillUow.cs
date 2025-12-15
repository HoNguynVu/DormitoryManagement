using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface IUtilityBillUow : ITransactionManager
    {
        IUtilityBillRepository UtilityBills { get; }
        IParameterRepository Parameters { get; }
        IContractRepository Contracts { get; }
        IAccountRepository Accounts { get; }
        INotificationRepository Notifications { get; }
        IStudentRepository Students { get; }
    }
}
