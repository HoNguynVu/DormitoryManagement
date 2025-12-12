using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class RoomTypeRepository : GenericRepository<RoomType>, IRoomTypeRepository
    {
        public RoomTypeRepository(DormitoryDbContext context) : base(context)
        {
        }

        public async Task<RoomType?> GetRoomTypeByRoomId(string roomId)
        {
            return await _context.Rooms
                .Where(r => r.RoomID == roomId)
                .Select(r => r.RoomType)
                .FirstOrDefaultAsync();
        }
    }
}
