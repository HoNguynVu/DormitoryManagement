using BusinessObject.DTOs.ViolationDTOs;

namespace API.Services.Interfaces
{
    public interface IViolationService
    {
        Task<(bool Success, string Message, int StatusCode, ViolationResponse? Data)> CreateViolationAsync(CreateViolationRequest request);
        Task<(bool Success, string Message, int StatusCode)> UpdateViolationAsync(UpdateViolationRequest request);
        Task<(bool Success, string Message, int StatusCode, IEnumerable<ViolationResponse>? Data)> GetViolationsByStudentIdAsync(string studentId);
        Task<(bool Success, string Message, int StatusCode, IEnumerable<ViolationResponse>? Data)> GetAllViolationsAsync();
        Task<(bool Success, string Message, int StatusCode, IEnumerable<ViolationResponse>? Data)> GetPendingViolationsAsync();
    }
}