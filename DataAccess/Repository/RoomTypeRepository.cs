using DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class RoomTypeRepository : IRoomTypeRepository
    {
        private readonly DormitoryDbContext _context;
        public RoomTypeRepository(DormitoryDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<RoomType>> GetAllRoomTypes()
        {
            return await _context.RoomTypes.ToListAsync();
        }
        public async Task<RoomType?> GetRoomTypeById(string roomTypeId)
        {
            return await _context.RoomTypes.FindAsync(roomTypeId);
        }
        public async Task<RoomType?> GetRoomTypeByRoomId(string roomId)
        {
            return await _context.Rooms
        .Where(r => r.RoomID == roomId)
        .Select(r => r.RoomType)
        .FirstOrDefaultAsync();
        }
        public void AddRoomType(RoomType roomType)
        {
            _context.RoomTypes.Add(roomType);
        }
        public void UpdateRoomType(RoomType roomType)
        {
            _context.RoomTypes.Update(roomType);
        }
        public void DeleteRoomType(RoomType roomType)
        {
            _context.RoomTypes.Remove(roomType);
        }
    }
}
