using BusinessObject.DTOs.BuildingManagerDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Services.Interfaces
{
    public interface IBuildingManagerService
    {
        Task<IEnumerable<BuildingManagerDto>> GetAllManagersAsync();
        Task<BuildingManagerDto?> GetManagerByIdAsync(string managerId);
    }
}
