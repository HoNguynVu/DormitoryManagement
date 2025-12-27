using DataAccess.Interfaces;

namespace API.UnitOfWorks
{
    public interface IRoomEquipmentUow : ITransactionManager
    {
        public IRoomEquipmentRepository RoomEquipments { get; }

        public IEquipmentRepository Equipments { get; }
    }
}
