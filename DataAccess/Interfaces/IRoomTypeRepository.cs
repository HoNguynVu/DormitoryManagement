using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;

namespace DataAccess.Interfaces
{
    public interface IRoomTypeRepository
    {
        Task<IEnumerable<RoomType>> GetAllRoomTypes();
        Task<RoomType?> GetRoomTypeById(string roomTypeId);
        void AddRoomType(RoomType roomType);
        void UpdateRoomType(RoomType roomType);
        void DeleteRoomType(RoomType roomType);
    }
}
