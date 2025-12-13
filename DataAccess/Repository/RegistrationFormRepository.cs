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
    public class RegistrationFormRepository : GenericRepository<RegistrationForm>, IRegistrationFormRepository
    {
        public RegistrationFormRepository(DormitoryDbContext context) : base(context)
        {
        }

        public async Task<int> CountRegistrationFormsByRoomId(string roomId)
        {
            var threshold = DateTime.UtcNow.AddMinutes(-10);

            return await _dbSet
                .CountAsync(f => f.RoomID == roomId &&
                                f.Status == "Pending" &&
                                f.RegistrationTime >= threshold);
        }

        public async Task<Dictionary<string, int>> CountPendingFormsByRoomAsync()
        {
            var threshold = DateTime.UtcNow.AddMinutes(-15);

            var query = await _dbSet
                .Where(f => f.Status == "Pending" && f.RegistrationTime >= threshold)
                .GroupBy(f => f.RoomID)
                .Select(g => new { RoomId = g.Key, Count = g.Count() })
                .ToListAsync();

            return query.ToDictionary(x => x.RoomId, x => x.Count);
        }
    }
}
