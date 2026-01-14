using BusinessObject.DTOs.ViolationDTOs;

namespace API.Services.Interfaces
{
    public interface IViolationService
    {
        Task<(bool Success, string Message, int StatusCode, ViolationResponse? Data)> CreateViolationAsync(CreateViolationRequest request);
        Task<(bool Success, string Message, int StatusCode)> UpdateViolationAsync(UpdateViolationRequest request);
        Task<(bool Success, string Message, int StatusCode)> DeleteViolationAsync(string violationId, string managerAccountId);
        Task<(bool Success, string Message, int StatusCode, IEnumerable<ViolationResponse>? Data)> GetViolationsByStudentIdAsync(string studentId);
        Task<(bool Success, string Message, int StatusCode, IEnumerable<ViolationResponse>? Data)> GetViolationsByStudentAccountIdAsync(string accountId);
        Task<(bool Success, string Message, int StatusCode, IEnumerable<ViolationResponse>? Data)> GetAllViolationsAsync();
        Task<(bool Success, string Message, int StatusCode, IEnumerable<ViolationResponse>? Data)> GetPendingViolationsAsync();
        Task<(bool Success, string Message, int StatusCode, IEnumerable<ViolationStats>? Data)> GetViolationStatsByManagerAsync(string accountId);
        Task<(bool Success, string Message, int StatusCode, IEnumerable<ViolationResponse>? Data)> GetAllViolationsByManagerAsync(string accountId);
    }
}