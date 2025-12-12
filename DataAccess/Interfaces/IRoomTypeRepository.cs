using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;

namespace DataAccess.Interfaces
{
    public interface IRoomTypeRepository : IGenericRepository<RoomType>
    {
        Task<RoomType?> GetRoomTypeByRoomId(string roomId);
    }
}
