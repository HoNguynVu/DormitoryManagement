using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface IStudentUow : ITransactionManager
    {
        public IStudentRepository Students { get; }
        public IRelativeRepository Relatives { get; }
    }
}
