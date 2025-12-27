using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class BuildingManagerRepository : GenericRepository<BuildingManager>, IBuildingManagerRepository
    {
        public BuildingManagerRepository(DormitoryDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<BuildingManager>> GetAllWithBuildingsAsync()
        {
            return await _dbSet
                .Include(bm => bm.Buildings)
                .ToListAsync();
        }

        // Ensure single manager load includes Buildings
        public override async Task<BuildingManager?> GetByIdAsync(string id)
        {
            return await _dbSet
                .Include(bm => bm.Buildings)
                .FirstOrDefaultAsync(bm => bm.ManagerID == id);
        }

        public async Task<BuildingManager?> GetByAccountIdAsync(string accountId)
        {
            return await _dbSet
                .Include(bm => bm.Buildings)
                .FirstOrDefaultAsync(bm => bm.AccountID == accountId);
        }
    }
}
