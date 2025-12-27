using BusinessObject.Entities;
using BusinessObject.DTOs.ReportDTOs;
using BusinessObject.DTOs.RoomDTOs;

namespace API.Services.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<StudentPriorityDto>> GetStudentsByPriorityAsync(string? priorityId = null);
        Task<IEnumerable<ExpiredContractDto>> GetExpiredContractsAsync(DateOnly olderThan);
        Task<IEnumerable<StudentContractDto>> GetContractsByStudentAsync(string studentId);
        Task<IEnumerable<EquipmentStatusDto>> GetEquipmentStatusByRoomAsync(string roomId);
        Task<IEnumerable<AvailableRoomDto>> GetAvailableRoomsAsync(RoomFilterDto filter);

        Task<OverviewDashBoardDto> GetOverviewDashBoard();
    }
}
