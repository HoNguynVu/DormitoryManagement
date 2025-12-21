using BusinessObject.DTOs.ContractDTOs;
using BusinessObject.DTOs.MaintenanceDTOs;

namespace API.Services.Interfaces
{
    public interface IMaintenanceService
    {
        // Sinh viên gửi báo cáo
        Task<(bool Success, string Message, int StatusCode)> CreateRequestAsync(CreateMaintenanceDto dto);

        // Lấy danh sách (cho SV xem lịch sử hoặc Manager xem tất cả)
        Task<(bool Success, string Message, int StatusCode, object? Data)> GetRequestsAsync(string? studentId, string? status);

        // Cập nhật trạng thái yêu cầu sửa chữa
        Task<(bool Success, string Message, int StatusCode)> UpdateStatusAsync(UpdateMaintenanceStatusDto dto);


        Task<(bool Success, string Message, int StatusCode, IEnumerable<SummaryMaintenanceDto> dto)> GetMaintenanceFiltered(string? keyword, string? status,string? equipmentName);
        Task<(bool Success, string Message, int StatusCode, DetailMaintenanceDto dto)> GetMaintenanceDetail(string maintenanceId);
        Task<(bool Success, string Message, int StatusCode, Dictionary<string,int> list)> GetOverviewMaintenance();

    }
}
