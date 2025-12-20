using BusinessObject.DTOs.BuildingDTOs;
using BusinessObject.Entities;

namespace API.Services.Interfaces
{
    public interface IBuildingService
    {
        Task<(bool Success, string Message, int StatusCode, IEnumerable<GetBuildingDTO>)> GetAllBuildingAsync();
    }
}
