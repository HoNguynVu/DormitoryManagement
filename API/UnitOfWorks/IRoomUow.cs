using DataAccess.Interfaces;
using System.Threading.Tasks;

namespace API.UnitOfWorks
{
    public interface IRoomUow
    {
        IRoomRepository Rooms { get; }
        IRegistrationFormRepository RegistrationForms { get; }
        IRoomTypeRepository RoomTypes { get; }

        // Allow services to persist changes made via repositories
        Task<int> SaveChangesAsync();

        // Check master data existence
        Task<bool> BuildingExistsAsync(string buildingId);
    }
}
