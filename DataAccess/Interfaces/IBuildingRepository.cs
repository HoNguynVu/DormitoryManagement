using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;

namespace DataAccess.Interfaces
{
    public interface IBuildingRepository : IGenericRepository<Building>
    {
        Task<Building?> GetByManagerId(string managerId);
        Task<bool> IsManagerAssigned(string managerId);
    }
}
