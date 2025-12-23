using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface IStudentUow
    {
        public IStudentRepository Students { get; }
        public IRelativeRepository Relatives { get; }
    }
}
