using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class RoomEquipmentRepository : GenericRepository<RoomEquipment>, IRoomEquipmentRepository
    {
        public RoomEquipmentRepository(DormitoryDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<RoomEquipment>> GetEquipmentsByRoomIdAsync(string roomId)
        {
            return await _dbSet
                .Include(re => re.Equipment)
                .Include(re => re.Room)
                .Where(re => re.RoomID == roomId)
                .ToListAsync();
        }
        public async Task<bool> IsEquipmentInRoomAsync(string roomId, string equipmentId)
        {
            return await _dbSet
                .AnyAsync(re => re.RoomID == roomId && re.EquipmentID == equipmentId);
        }

        public async Task<RoomEquipment?> GetGoodRoomEquipmentAsync(string roomId, string equipmentId)
        {
            return await _dbSet
                .Include(re => re.Equipment)
                .Include(re => re.Room)
                .Where(re => re.RoomID == roomId && re.EquipmentID == equipmentId && re.Status == "Good")
                .FirstOrDefaultAsync();
        }
    }
}
