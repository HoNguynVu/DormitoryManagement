using BusinessObject.DTOs.ReportDTOs;
using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IBuildingRepository : IGenericRepository<Building>
    {
        Task<Building?> GetByManagerId(string managerId);
        Task<bool> IsManagerAssigned(string managerId);
        Task<List<BuildingPerformanceDto>> GetBuildingPerformanceAsync();
    }
}
