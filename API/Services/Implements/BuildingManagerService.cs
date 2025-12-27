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

        public async Task<(bool Success, string Message, int StatusCode, DashboardStatsDTO Data)> GetDashboardStatsAsync(string accountId)
        {
            try
            {
                var manager = await _buildingUow.BuildingManagers.GetByAccountIdAsync(accountId);
                if (manager == null)
                {
                    return (false, "Building manager not found", 404, null);
                }
                var dashboardStats = new DashboardStatsDTO();
                var roomCount = await _buildingUow.Rooms.GetRoomCountsByManagerIdAsync(manager.ManagerID);
                dashboardStats.CountRooms = roomCount.Total;
                dashboardStats.AvailableRooms = roomCount.Available;
                var billsStats = await _buildingUow.UtilityBills.GetUnpaidBillStatsByManagerIdAsync(manager.ManagerID);
                dashboardStats.UnpaidUtilityBills = billsStats.Count;
                dashboardStats.TotalUnpaidAmount = billsStats.TotalAmount;
                var studentCount = await _buildingUow.Students.CountStudentByManagerIdAsync(manager.ManagerID);
                dashboardStats.TotalStudents = studentCount;
                var unresolvedRequests = await _buildingUow.Maintenances.CountUnresolveRequestsByManagerIdAsync(manager.ManagerID);
                dashboardStats.UnResolveRequests = unresolvedRequests;
                return (true, "Dashboard statistics retrieved successfully", 200, dashboardStats);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}", 500, null);
            }
        }
    }
}
