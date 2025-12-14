using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class NotificationRepository : GenericRepository<Notification>, Interfaces.INotificationRepository
    {
        public NotificationRepository(Models.DormitoryDbContext context) : base(context)
        {
        }
    }
}
