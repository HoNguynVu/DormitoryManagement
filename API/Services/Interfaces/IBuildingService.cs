using BusinessObject.DTOs.BuildingDTOs;
using BusinessObject.DTOs.RoomDTOs;
using BusinessObject.Entities;

namespace API.Services.Interfaces
{
    public interface IBuildingService
    {
        Task<(bool Success, string Message, int StatusCode, IEnumerable<GetBuildingDTO>)> GetAllBuildingAsync();
        Task<(bool Success, string Message, int StatusCode, IEnumerable<BuildingWithManagerDto> Data)> GetBuildingWithManagerAsync(string buildingId);
        Task<(bool Success, string Message, int StatusCode, IEnumerable<RoomResponseDto> Data)> GetRoomByManagerId(string managerId);
        Task<(bool Success, string Message, int StatusCode)> CreateBuildingAsync(CreateBuildingDto createDto);
        Task<(bool Success, string Message, int StatusCode)> UpdateBuildingAsync(UpdateBuildingDto updateDto);
        Task<(bool Success, string Message, int StatusCode, AllBuildingStatsForAdmin Data)> GetBuildingsStats();
    }
}
