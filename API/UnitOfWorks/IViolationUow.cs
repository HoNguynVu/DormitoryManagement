using DataAccess.Interfaces;
namespace API.UnitOfWorks
{
    public interface IViolationUow : ITransactionManager
    {
        IViolationRepository Violations { get; }
        IStudentRepository Students { get; }
        IContractRepository Contracts { get; }
    }
}
