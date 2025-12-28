using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface IRoomTypeUow : ITransactionManager
    {
        IRoomTypeRepository RoomTypes { get; }
        IRoomRepository Rooms { get; }
    }
}
