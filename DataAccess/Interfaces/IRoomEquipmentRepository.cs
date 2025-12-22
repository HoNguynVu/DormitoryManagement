using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IRoomEquipmentRepository : IGenericRepository<RoomEquipment>
    {
        Task<IEnumerable<RoomEquipment>> GetEquipmentsByRoomIdAsync(string roomId);
        Task<bool> IsEquipmentInRoomAsync(string roomId, string equipmentId);

        Task<RoomEquipment?> GetGoodRoomEquipmentAsync(string roomId, string equipmentId);
    }
}
