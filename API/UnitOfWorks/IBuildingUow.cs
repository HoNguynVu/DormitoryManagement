using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface IBuildingUow : ITransactionManager
    {
        public IBuildingManagerRepository BuildingManagers { get; }
        public IRoomRepository Rooms { get; }
    }
}
