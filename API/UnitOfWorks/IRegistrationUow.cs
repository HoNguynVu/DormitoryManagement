using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface IRegistrationUow : ITransactionManager
    {
        IRegistrationFormRepository RegistrationForms { get; }
        IContractRepository Contracts { get; }
        IRoomRepository Rooms { get; }
    }
}
