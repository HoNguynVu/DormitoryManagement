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
    public class EquipmentRepository : GenericRepository<Equipment>, IEquipmentRepository
    {
        public EquipmentRepository(DormitoryDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Equipment>> GetEquipmentsByRoomIdAsync(string roomId)
        {
            return await _dbSet.Where(r => r.RoomID == roomId ).ToListAsync();
        }
    }
}
