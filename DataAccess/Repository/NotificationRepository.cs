using BusinessObject.Entities;
using DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(Models.DormitoryDbContext context) : base(context)
        {
        }

        public async Task<List<Notification>> GetLastestNotificationsByAccountIdAsync(string accountId)
        {
            return await Task.Run(() =>
            {
                return _context.Notifications
                    .Where(n => n.AccountID == accountId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(10)
                    .ToList();
            });
        }
    }
}
