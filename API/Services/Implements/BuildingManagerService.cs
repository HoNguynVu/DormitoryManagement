using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.Entities;
using BusinessObject.DTOs.BuildingManagerDTOs;
using BusinessObject.Helpers;

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
                CitizenId = m.CitizenId,
                DateOfBirth = m.DateOfBirth ?? DateTime.MinValue,
                Address = m.Address,
                BuildingDto = new BuildingDto
                {
                    BuildingID = m.Buildings?.FirstOrDefault()?.BuildingID ?? "",
                    BuildingName = m.Buildings?.FirstOrDefault()?.BuildingName ?? ""
                }
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
                CitizenId = m.CitizenId,
                DateOfBirth = m.DateOfBirth ?? DateTime.MinValue,
                Address = m.Address,
                BuildingDto = new BuildingDto
                {
                    BuildingID = m.Buildings?.FirstOrDefault()?.BuildingID ?? "",
                    BuildingName = m.Buildings?.FirstOrDefault()?.BuildingName ?? ""
                }
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

        public async Task<(bool Success, string Message, int StatusCode, PagedResult<ReceiptForManagerDTO>? Data)> GetReceiptsAsync(GetReceiptRequest request)
        {
            try
            {
                var manager = await _buildingUow.BuildingManagers.GetByAccountIdAsync(request.AccountId);
                if (manager == null)
                {
                    return (false, "Building manager not found", 404, null);
                }

                var pagedReceipts = await _buildingUow.Receipts.GetReceiptsByManagerPagedAsync(manager.ManagerID, request.PageIndex, request.PageSize);

                var receiptDtos = pagedReceipts.Items.Select(r =>
                {
                    var activeContract = r.Student?.Contracts?
                        .FirstOrDefault(c => c.ContractStatus == "Active");

                    return new ReceiptForManagerDTO
                    {
                        ReceiptId = r.ReceiptID,
                        StudentId = r.StudentID,
                        StudentName = r.Student?.FullName ?? "N/A", 

                        RoomName = activeContract?.Room?.RoomName ?? "N/A",

                        PaymentType = r.PaymentType,
                        Amount = r.Amount,
                        CreatedDate = r.PrintTime,
                        Status = r.Status
                    };
                });

                var pagedResultDto = new PagedResult<ReceiptForManagerDTO>(
                    receiptDtos.ToList(),
                    pagedReceipts.TotalItems, 
                    pagedReceipts.PageIndex,
                    pagedReceipts.PageSize
                );

                return (true, "Receipts retrieved successfully", 200, pagedResultDto);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}", 500, null);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode)> UpdateManagerAsync(UpdateBuildingManagerDto updateDto)
        {
            await _buildingUow.BeginTransactionAsync();
            try
            {
                var manager = await _buildingUow.BuildingManagers.GetByIdAsync(updateDto.ManagerID);
                if (manager == null)
                {
                    return (false, "Building manager not found", 404);
                }
                manager.FullName = updateDto.FullName;
                manager.CitizenId = updateDto.CitizenId;
                manager.DateOfBirth = updateDto.DateOfBirth;
                manager.PhoneNumber = updateDto.PhoneNumber;
                manager.Address = updateDto.Address;
                _buildingUow.BuildingManagers.Update(manager);
                await _buildingUow.CommitAsync();
                return (true, "Building manager updated successfully", 200);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}", 500);
            }
        }
    }
}
