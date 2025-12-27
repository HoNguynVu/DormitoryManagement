using BusinessObject.DTOs.BuildingManagerDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Services.Interfaces
{
    public interface IBuildingManagerService
    {
        Task<IEnumerable<BuildingManagerDto>> GetAllManagersAsync();
        Task<BuildingManagerDto?> GetManagerByIdAsync(string managerId);
        Task<(bool Success, string Message, int StatusCode, DashboardStatsDTO Data )> GetDashboardStatsAsync(string accountId);
    }
}
