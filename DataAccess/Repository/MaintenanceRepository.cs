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

        public async Task<IEnumerable<MaintenanceRequest>> GetMaintenanceFilteredAsync(string? studentId, string? status)
        {
            return await _context.MaintenanceRequests
               .Include(m => m.Student)
               .Include(m => m.Room)
               .Include(m=>m.Equipment)
               .Where(m => m.StudentID.Contains(studentId??"") || m.Status.Contains(status??""))
               .ToListAsync();
        }
    }
}
