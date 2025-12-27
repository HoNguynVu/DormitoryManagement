using BusinessObject.DTOs.BuildingManagerDTOs;
using BusinessObject.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Services.Interfaces
{
    public interface IBuildingManagerService
    {
        Task<IEnumerable<BuildingManagerDto>> GetAllManagersAsync();
        Task<BuildingManagerDto?> GetManagerByIdAsync(string managerId);
        Task<(bool Success, string Message, int StatusCode, DashboardStatsDTO Data )> GetDashboardStatsAsync(string accountId);
        Task<(bool Success, string Message, int StatusCode, PagedResult<ReceiptForManagerDTO> Data)> GetReceiptsAsync(GetReceiptRequest request);
        Task<(bool Success, string Message, int StatusCode)> UpdateManagerAsync(UpdateBuildingManagerDto updateDto);
        Task<(bool Success, string Message, int StatusCode)> CreateManagerAsync(CreateManagerDto createDto);
        Task<(bool Success, string Message, int StatusCode)> DeleteManagerAsync(string managerId);
    }
}
