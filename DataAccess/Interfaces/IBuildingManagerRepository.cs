using BusinessObject.DTOs.ReportDTOs;
using BusinessObject.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IBuildingManagerRepository : IGenericRepository<BuildingManager>
    {
        Task<IEnumerable<BuildingManager>> GetAllWithBuildingsAsync();
        Task<BuildingManager?> GetByAccountIdAsync(string accountId);
        Task<GrowthStatDto> GetBuildingManagerGrowthStatsAsync();
    }
}
