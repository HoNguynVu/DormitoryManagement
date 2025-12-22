using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface IEquipmentUow : ITransactionManager
    {
        public IEquipmentRepository Equipments { get; }
        public IRoomRepository Rooms { get; }
    }
}
