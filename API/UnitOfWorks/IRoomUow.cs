using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface IRoomUow
    {
        IRoomRepository Rooms { get; }
        IRegistrationFormRepository RegistrationForms { get; }
        IRoomTypeRepository RoomTypes { get; }
    }
}
