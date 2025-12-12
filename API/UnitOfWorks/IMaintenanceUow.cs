using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface IMaintenanceUow
    {
        public IMaintenanceRepository Maintenances { get; }
        public IRoomRepository Rooms { get; }
        public IStudentRepository Students { get; }
    }
}
