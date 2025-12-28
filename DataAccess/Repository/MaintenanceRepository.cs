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
    public class MaintenanceRepository : GenericRepository<MaintenanceRequest>, IMaintenanceRepository
    {
        public MaintenanceRepository(DormitoryDbContext context) : base(context)
        {
        }

        public async Task<MaintenanceRequest?> GetMaintenanceByIdAsync(string maintenanceId)
        {
            return await _dbSet
                .Include(m => m.Student)
                .ThenInclude(m=>m.Account)
                .Include(m => m.Room)
                .Include(m => m.Equipment)
                .FirstOrDefaultAsync(m => m.RequestID == maintenanceId);
        }

        public async Task<IEnumerable<MaintenanceRequest>> GetMaintenanceByStudentIdAsync(string studentId)
        {
            return await _dbSet
               .Include(m => m.Student)
               .Include(m => m.Room)
               .Include(m=>m.Equipment)
               .Where(m => m.StudentID==studentId)
               .AsNoTracking()
               .ToListAsync();
        }

        public async Task<IEnumerable<MaintenanceRequest>> GetMaintenanceFilteredAsync(string? keyword, string? status, string? equipmentName,string? buildingId)
        {
            var query = _dbSet
                .Include(m => m.Student)
                .Include(m => m.Room)
                    .ThenInclude(r=>r.Building)
                .Include(m => m.Equipment)
                .AsQueryable();
            if(!string.IsNullOrEmpty(buildingId))
                query = query.Where(m=>m.Room.Building.BuildingID==buildingId);
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(m =>
                    m.RequestID.Contains(keyword) ||
                    m.Student.FullName.Contains(keyword) ||
                    m.Room.RoomName.Contains(keyword));
            }
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(m => m.Status == status);
            }
            if (!string.IsNullOrEmpty(equipmentName))
            {
                query = query.Where(m => m.Equipment != null && m.Equipment.EquipmentName.Contains(equipmentName));
            }
            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<MaintenanceRequest?> GetMaintenanceDetailAsync(string maintenanceId)
        {
            return await _dbSet
                .Include(m => m.Student)
                .Include(m => m.Room)
                .Include(m => m.Equipment)
                .FirstOrDefaultAsync(m => m.RequestID == maintenanceId);
        }

        public async Task<int> CountUnresolveRequestsByManagerIdAsync(string managerId)
        {
            var count = await _dbSet
                .Include(m => m.Room)
                .Where(m => m.Room.Building.ManagerID == managerId && m.Status != "Completed")
                .CountAsync();
            return count;
        }
    }
}
