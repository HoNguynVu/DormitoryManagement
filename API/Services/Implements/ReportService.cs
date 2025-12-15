using API.UnitOfWorks;
using BusinessObject.Entities;
using BusinessObject.DTOs.ReportDTOs;
using API.Services.Interfaces;

namespace API.Services.Implements
{
    public class ReportService : IReportService
    {
        private readonly IContractUow _contractUow;
        private readonly IMaintenanceUow _maintenanceUow;

        public ReportService(IContractUow contractUow, IMaintenanceUow maintenanceUow)
        {
            _contractUow = contractUow;
            _maintenanceUow = maintenanceUow;
        }

        public async Task<IEnumerable<Student>> GetStudentsByPriorityAsync(string? priorityId = null)
        {
            var students = await _contractUow.Students.GetAllAsync();
            if (!string.IsNullOrWhiteSpace(priorityId))
            {
                return students.Where(s => s.PriorityID == priorityId);
            }
            return students.Where(s => !string.IsNullOrWhiteSpace(s.PriorityID));
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
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                ContractStatus = c.ContractStatus
            });
        }

        public async Task<IEnumerable<EquipmentStatusDto>> GetEquipmentStatusByRoomAsync(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId)) return Enumerable.Empty<EquipmentStatusDto>();

            // Prefer maintenance uow if it exposes Equipments repo
            if (_maintenanceUow != null)
            {
                var room = await _maintenanceUow.Rooms.GetByIdAsync(roomId);
                if (room == null) return Enumerable.Empty<EquipmentStatusDto>();

                // Load equipments via room navigation if available
                var equipments = room.Equipment ?? Enumerable.Empty<BusinessObject.Entities.Equipment>();

                return equipments.Select(e => new EquipmentStatusDto
                {
                    EquipmentID = e.EquipmentID,
                    EquipmentName = e.EquipmentName,
                    Status = e.Status ?? string.Empty,
                    RoomID = e.RoomID
                });
            }

            // Fallback: try contractUow rooms
            var altRoom = await _contractUow.Rooms.GetByIdAsync(roomId);
            if (altRoom == null) return Enumerable.Empty<EquipmentStatusDto>();
            var altEquipments = altRoom.Equipment ?? Enumerable.Empty<BusinessObject.Entities.Equipment>();
            return altEquipments.Select(e => new EquipmentStatusDto
            {
                EquipmentID = e.EquipmentID,
                EquipmentName = e.EquipmentName,
                Status = e.Status ?? string.Empty,
                RoomID = e.RoomID
            });
        }
    }
}
