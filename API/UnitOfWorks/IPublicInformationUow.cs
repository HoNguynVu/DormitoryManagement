using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface IPublicInformationUow
    {
        public IPriorityRepository Priorities { get; }
        public ISchoolRepository Schools { get; }
    }
}
