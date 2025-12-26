using BusinessObject.DTOs.ContractDTOs;
using BusinessObject.DTOs.MaintenanceDTOs;

namespace API.Services.Interfaces
{
    public interface IMaintenanceService
    {
        // Sinh viên gửi báo cáo
        Task<(bool Success, string Message, int StatusCode,string? requestMaintenanceId)> CreateRequestAsync(CreateMaintenanceDto dto);

        // Lấy danh sách (cho SV xem lịch sử )
        Task<(bool Success, string Message, int StatusCode, IEnumerable<SummaryMaintenanceDto> dto)> GetRequestsByStudentIdAsync(string studentId);

        // Cập nhật trạng thái yêu cầu sửa chữa
        Task<(bool Success, string Message, int StatusCode)> UpdateStatusAsync(UpdateMaintenanceStatusDto dto);


        Task<(bool Success, string Message, int StatusCode, IEnumerable<SummaryMaintenanceDto> dto)> GetMaintenanceFiltered(string? keyword, string? status,string? equipmentName);
        Task<(bool Success, string Message, int StatusCode, DetailMaintenanceDto dto)> GetMaintenanceDetail(string maintenanceId);
        Task<(bool Success, string Message, int StatusCode, Dictionary<string,int> list)> GetOverviewMaintenance();
        Task<(bool Success, string Message, int StatusCode)> ConfirmPaymentMaintenanceFee(string maintainanceId);

    }
}
