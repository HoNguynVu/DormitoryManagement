using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Interfaces;
using BusinessObject.Entities;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class RoomRepository : IRoomRepository
    {
        private readonly DormitoryDbContext _context;
        public RoomRepository(DormitoryDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Room>> GetAllRooms()
        {
            return await _context.Rooms.ToListAsync();
        }
        public async Task<Room?> GetRoomById(string roomId)
        {
            return await _context.Rooms.FindAsync(roomId);
        }
        public void AddRoom(Room room)
        {
            _context.Rooms.Add(room);
        }
        public void UpdateRoom(Room room)
        {
            _context.Rooms.Update(room);
        }
        public void DeleteRoom(Room room)
        {
            _context.Rooms.Remove(room);
        }
    }
}
