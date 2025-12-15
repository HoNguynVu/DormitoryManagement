using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.Entities;
using BusinessObject.DTOs.BuildingManagerDTOs;

namespace API.Services.Implements
{
    public class BuildingManagerService : IBuildingManagerService
    {
        private readonly IBuildingUow _buildingUow;
        public BuildingManagerService(IBuildingUow buildingUow)
        {
            _buildingUow = buildingUow;
        }

        public async Task<IEnumerable<BuildingManagerDto>> GetAllManagersAsync()
        {
            var managers = await _buildingUow.BuildingManagers.GetAllWithBuildingsAsync();

            return managers.Select(m => new BuildingManagerDto
            {
                ManagerID = m.ManagerID,
                FullName = m.FullName,
                Email = m.Email,
                PhoneNumber = m.PhoneNumber,
                Address = m.Address,
                Buildings = m.Buildings?.Select(b => new BuildingDto { BuildingID = b.BuildingID, BuildingName = b.BuildingName }) ?? Array.Empty<BuildingDto>()
            });
        }

        public async Task<BuildingManagerDto?> GetManagerByIdAsync(string managerId)
        {
            var m = await _buildingUow.BuildingManagers.GetByIdAsync(managerId);
            if (m == null) return null;

            return new BuildingManagerDto
            {
                ManagerID = m.ManagerID,
                FullName = m.FullName,
                Email = m.Email,
                PhoneNumber = m.PhoneNumber,
                Address = m.Address,
                Buildings = m.Buildings?.Select(b => new BuildingDto { BuildingID = b.BuildingID, BuildingName = b.BuildingName }) ?? Array.Empty<BuildingDto>()
            };
        }
    }
}
