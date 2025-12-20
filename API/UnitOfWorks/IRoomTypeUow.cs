using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface IRoomTypeUow
    {
        IRoomTypeRepository RoomTypes { get; }
    }
}
