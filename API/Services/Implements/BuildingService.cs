using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.BuildingDTOs;
using BusinessObject.Entities;

namespace API.Services.Implements
{
    public class BuildingService : IBuildingService
    {
        private readonly IBuildingUow _buildingUow;
        public BuildingService(IBuildingUow buildingUow)
        {
            _buildingUow = buildingUow;
        }
        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<GetBuildingDTO>)> GetAllBuildingAsync()
        {
            try
            {
                var allBuildings = await _buildingUow.Buildings.GetAllAsync();
                var buildingDtos = allBuildings.Select(b => new GetBuildingDTO
                {
                    BuildingID = b.BuildingID,
                    BuildingName = b.BuildingName
                });
                return (true, "Buildings retrieved successfully.", 200, buildingDtos);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred while retrieving buildings: {ex.Message}", 500, Enumerable.Empty<GetBuildingDTO>());
            }
        }
    }
}
