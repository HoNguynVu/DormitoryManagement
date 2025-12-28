using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.BuildingDTOs;
using BusinessObject.DTOs.RoomDTOs;
using BusinessObject.Entities;

namespace API.Services.Implements
{
    public class BuildingService : IBuildingService
    {
        private readonly IBuildingUow _buildingUow;
        private readonly IRoomService _roomService;
        public BuildingService(IBuildingUow buildingUow, IRoomService roomService)
        {
            _buildingUow = buildingUow;
            _roomService = roomService;
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


        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<BuildingWithManagerDto> Data)> GetBuildingWithManagerAsync(string buildingId)
        {
            try
            {
                var building = await _buildingUow.Buildings.GetByIdAsync(buildingId);
                if (building == null)
                {
                    return (false, "Building not found.", 404, Enumerable.Empty<BuildingWithManagerDto>());
                }
                var manager = await _buildingUow.BuildingManagers.GetByIdAsync(building.ManagerID);
                if (manager == null)
                {
                    return (false, "Manager not found for the specified building.", 404, Enumerable.Empty<BuildingWithManagerDto>());
                }
                var buildingWithManagerDto = new BuildingWithManagerDto
                {
                    BuildingID = building.BuildingID,
                    BuildingName = building.BuildingName,
                    ManagerID = manager?.ManagerID ?? string.Empty,
                    ManagerName = manager?.FullName ?? string.Empty
                };
                return (true, "Building with manager retrieved successfully.", 200, new List<BuildingWithManagerDto> { buildingWithManagerDto });
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred while retrieving the building with manager: {ex.Message}", 500, Enumerable.Empty<BuildingWithManagerDto>());
            }
        }

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<RoomResponseDto> Data)> GetRoomByManagerId(string managerId)
        {
            try
            {
                var manager = await _buildingUow.BuildingManagers.GetByIdAsync(managerId);
                if (manager == null)
                {
                    return (false, "Manager not found.", 404, Enumerable.Empty<RoomResponseDto>());
                }
                var rooms = await _roomService.GetAllRoomsForManagerAsync(manager.AccountID);
                return (true, "Rooms retrieved successfully.", 200, rooms.Item4);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred while retrieving rooms: {ex.Message}", 500, Enumerable.Empty<BusinessObject.DTOs.RoomDTOs.RoomResponseDto>());
            }
        }

        public async Task<(bool Success, string Message, int StatusCode)> CreateBuildingAsync(CreateBuildingDto createDto)
        {
            await _buildingUow.BeginTransactionAsync();
            try
            {
                var manager = await _buildingUow.BuildingManagers.GetByIdAsync(createDto.ManagerID);
                if (manager == null)
                {
                    await _buildingUow.RollbackAsync();
                    return (false, "Manager not found.", 404);
                }

                var isManagerAssigned = await _buildingUow.Buildings.IsManagerAssigned(createDto.ManagerID);
                if (isManagerAssigned)
                {
                    await _buildingUow.RollbackAsync();
                    return (false, "This manager is already assigned to another building.", 400);
                }

                var newBuilding = new Building
                {
                    BuildingID = "BLD-" + IdGenerator.GenerateUniqueSuffix(),
                    BuildingName = createDto.BuildingName,
                    ManagerID = createDto.ManagerID
                };
                _buildingUow.Buildings.Add(newBuilding);
                await _buildingUow.CommitAsync();
                return (true, "Building created successfully.", 201);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred while creating the building: {ex.Message}", 500);
            }
        }
    }
}
