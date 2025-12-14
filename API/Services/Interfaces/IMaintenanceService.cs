using BusinessObject.DTOs.MaintenanceDTOs;

namespace API.Services.Interfaces
{
    public interface IMaintenanceService
    {
        // Sinh viên gửi báo cáo
        Task<(bool Success, string Message, int StatusCode)> CreateRequestAsync(CreateMaintenanceDto dto);

        // Lấy danh sách (cho SV xem lịch sử hoặc Manager xem tất cả)
        Task<(bool Success, string Message, int StatusCode, object? Data)> GetRequestsAsync(string? studentId, string? status);

        Task<(bool Success, string Message, int StatusCode)> UpdateStatusAsync(UpdateMaintenanceStatusDto dto);
    }
}
