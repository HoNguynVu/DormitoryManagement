using API.UnitOfWorks;
using BusinessObject.Entities;
using BusinessObject.DTOs.ReportDTOs;
using API.Services.Interfaces;
using BusinessObject.DTOs.RoomDTOs;

namespace API.Services.Implements
{
    public class ReportService : IReportService
    {
        private readonly IContractUow _contractUow;
        private readonly IMaintenanceUow _maintenanceUow;
        private readonly IRegistrationUow _registrationUow;

        public ReportService(IContractUow contractUow, IMaintenanceUow maintenanceUow, IRegistrationUow registrationUow)
        {
            _contractUow = contractUow;
            _maintenanceUow = maintenanceUow;
            _registrationUow = registrationUow;
        }

        public async Task<IEnumerable<StudentPriorityDto>> GetStudentsByPriorityAsync(string? priorityId = null)
        {
            var students = await _contractUow.Students.GetStudentsWithPriorityAsync(priorityId);
            return students.Select(s => new StudentPriorityDto
            {
                StudentID = s.StudentID,
                FullName = s.FullName,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber,
                PriorityID = s.PriorityID,
                PriorityName = s.Priority.PriorityDescription
            });

        }

        public async Task<IEnumerable<ExpiredContractDto>> GetExpiredContractsAsync(DateOnly olderThan)
        {
            var contracts = await _contractUow.Contracts.GetExpiredContractsAsync(olderThan);
            return contracts.Select(c => new ExpiredContractDto
            {
                ContractID = c.ContractID,
                StudentID = c.StudentID,
                StudentName = c.Student?.FullName ?? string.Empty,
                RoomID = c.RoomID,
                RoomName = c.Room?.RoomName ?? string.Empty,
                EndDate = c.EndDate ?? DateOnly.MinValue,
                ContractStatus = c.ContractStatus
            });
        }

        public async Task<IEnumerable<StudentContractDto>> GetContractsByStudentAsync(string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId)) return Enumerable.Empty<StudentContractDto>();

            var contracts = await _contractUow.Contracts.FindAsync(c => c.StudentID == studentId);

            return contracts.Select(c => new StudentContractDto
            {
                ContractID = c.ContractID,
                StudentID = c.StudentID,
                StudentName = c.Student?.FullName ?? string.Empty,
                RoomID = c.RoomID,
                RoomName = c.Room?.RoomName ?? string.Empty,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                ContractStatus = c.ContractStatus
            });
        }

        public async Task<IEnumerable<EquipmentStatusDto>> GetEquipmentStatusByRoomAsync(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId)) return Enumerable.Empty<EquipmentStatusDto>();

            var equipments = await _maintenanceUow.RoomEquipments.GetEquipmentsByRoomIdAsync(roomId);
            return equipments.Select(e => new EquipmentStatusDto
            {
                EquipmentID = e.EquipmentID,
                EquipmentName = e.Equipment?.EquipmentName ?? "Khong xac dinh",
                Status = e.Status ?? "Chua cap nhat",
                Quantity = e.Quantity,
                RoomID = e.RoomID,
                RoomName = e.Room?.RoomName ?? "Khong xac dinh"
            });
        }

        public async Task<IEnumerable<AvailableRoomDto>> GetAvailableRoomsAsync(RoomFilterDto filter)
        {
            // reuse contractUow.Rooms.FindBySpecificationAsync if available, otherwise implement same logic
            var spec = DataAccess.Specifications.RoomSpecifications.ByFilter(filter);
            var rooms = await _contractUow.Rooms.FindBySpecificationAsync(spec);

            var pendingDict = await _registrationUow.RegistrationForms.CountPendingFormsByRoomAsync();
            var result = new List<AvailableRoomDto>();

            foreach (var room in rooms)
            {
                var activeCount = await _contractUow.Contracts.CountContractsByRoomIdAndStatus(room.RoomID, "Active");
                var pending = pendingDict.GetValueOrDefault(room.RoomID, 0);
                var occupied = Math.Max(room.CurrentOccupancy, activeCount);
                var availableBeds = Math.Max(0, room.Capacity - (occupied + pending));

                if (filter?.OnlyAvailable == true && availableBeds <= 0)
                    continue;

                result.Add(new AvailableRoomDto
                {
                    RoomID = room.RoomID,
                    RoomName = room.RoomName,
                    Capacity = room.Capacity,
                    Occupied = occupied,
                    AvailableBeds = availableBeds,
                    Price = room.RoomType?.Price ?? 0,
                    RoomType = room.RoomType?.TypeName ?? string.Empty
                });
            }

            return result;
        }

        public async Task<OverviewDashBoardDto> GetOverviewDashBoard()
        {
            var studentTask = await _contractUow.Students.GetStudentGrowthStatsAsync();

            // 2. Gọi hàm từ StaffRepo (Đã viết logic đếm Manager + % Tăng trưởng)
            var managerTask = await _contractUow.BuildingManagers.GetStaffGrowthStatsAsync();

            // 3. Lấy tổng số tòa nhà (Logic đơn giản đếm Count)
            var buildingTask = await _contractUow.BuildingManagers.CountAsync();

            // 4. Lấy doanh thu tháng này (Logic tính tổng tiền)
            // Giả sử lấy doanh thu từ ngày 1 của tháng hiện tại
            var startOfThisMonth = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
            var revenueTask = await _contractUow.Receipts.GetRevenueGrowthStatsAsync();


            // 5. Map dữ liệu vào DTO trả về
            return new OverviewDashBoardDto
            {
                TotalStudents = studentTask.TotalValue,
                RateStudent = (decimal)studentTask.GrowthPercent,

                // --- Dữ liệu Manager (Từ Repo mới) ---
                TotalManager = managerTask.TotalValue,
                RateManager = (decimal)managerTask.GrowthPercent,

                // --- Dữ liệu Tòa nhà & Doanh thu ---
                TotalBuilding = buildingTask,
                TotalRevenue = revenueTask.TotalMoney
            };
        }
    }
}
