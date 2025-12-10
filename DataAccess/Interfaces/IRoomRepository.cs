using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;

namespace DataAccess.Interfaces
{
    public interface IRoomRepository
    {
        Task<IEnumerable<Room>> GetAllRooms();
        Task<Room?> GetRoomById(string roomId);
        void AddRoom(Room room);
        void UpdateRoom(Room room);
        void DeleteRoom(Room room);
    }
}
