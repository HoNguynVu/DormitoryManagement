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

    }
}
