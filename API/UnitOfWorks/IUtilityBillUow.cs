using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface IUtilityBillUow : ITransactionManager
    {
        IUtilityBillRepository UtilityBills { get; }
        IParameterRepository Parameters { get; }
    }
}
