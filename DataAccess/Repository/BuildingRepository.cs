using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class BuildingRepository : GenericRepository<Building>, IBuildingRepository
    {
        public BuildingRepository(DormitoryDbContext context) : base(context)
        {
        }
        public async Task<Building?> GetByManagerId(string managerId)
        {
            return await _dbSet.Where(b => b.ManagerID == managerId).FirstOrDefaultAsync();
        }
        public async Task<bool> IsManagerAssigned(string managerId)
        {
            return await _dbSet.AnyAsync(b => b.ManagerID == managerId);
        }
    }
}
