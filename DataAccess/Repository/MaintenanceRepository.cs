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
    public class MaintenanceRepository : IMaintenanceRepository
    {
        private readonly DormitoryDbContext _context;
        public MaintenanceRepository(DormitoryDbContext context)
        {
            _context = context;
        }

        public void Add(MaintenanceRequest request)
        {
            _context.MaintenanceRequests.Add(request);
        }
        public void Update(MaintenanceRequest request)
        {
            _context.MaintenanceRequests.Update(request);
        }

        public async Task<MaintenanceRequest?> GetMaintenanceByIdAsync(string maintenanceId)
        {
            return await _context.MaintenanceRequests
                .Include(m => m.Student)
                .Include(m => m.Room)
                .FirstOrDefaultAsync(m => m.RequestID == maintenanceId);
        }

        public async Task<IEnumerable<MaintenanceRequest>> GetMaintenanceByStudentIdAsync(string studentId)
        {
            return await _context.MaintenanceRequests
               .Include(m => m.Student)
               .Include(m => m.Room)
               .Include(m=>m.Equipment)
               .Where(m => m.StudentID==studentId)
               .AsNoTracking()
               .ToListAsync();
        }

        public async Task<IEnumerable<MaintenanceRequest>> GetMaintenanceFilteredAsync(string? keyword, string? status, string? equipmentName)
        {
            var query = _context.MaintenanceRequests
                .Include(m => m.Student)
                .Include(m => m.Room)
                .Include(m => m.Equipment)
                .AsQueryable();
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
    }
}
