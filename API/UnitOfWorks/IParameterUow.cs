using DataAccess.Interfaces;
namespace API.UnitOfWorks
{
    public interface IParameterUow : ITransactionManager
    {
        public IParameterRepository Parameters { get; }
    }
}
