using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess.Repository
{
    public class RoomRepository : GenericRepository<Room>, IRoomRepository
    {
        public RoomRepository(DormitoryDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Room>> GetAllRoomsWithTypesAsync()
        {
            return await _dbSet
                .Include(r => r.RoomType)
                .Include(r => r.Building)
                .ToListAsync();
        }

        // ✅ Override để thêm eager loading
        public override async Task<Room?> GetByIdAsync(string id)
        {
            return await _dbSet
                .Include(r => r.RoomType)
                .Include(r => r.Building)
                .FirstOrDefaultAsync(r => r.RoomID == id);
        }

        // New: apply specification expression on DB side
        public async Task<IEnumerable<Room>> FindBySpecificationAsync(Expression<Func<Room, bool>> spec)
        {
            if (spec == null) return Enumerable.Empty<Room>();
            return await _dbSet
                .Include(r => r.RoomType)
                .Include(r => r.Building)
                .Where(spec)
                .ToListAsync();
        }
    }
}
