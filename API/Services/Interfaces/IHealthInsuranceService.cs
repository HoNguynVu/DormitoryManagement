using BusinessObject.Entities;

namespace API.Services.Interfaces
{
    public interface IHealthInsuranceService
    {
        //  Sinh viên đăng ký mua BHYT 
        Task<(bool Success, string Message, int StatusCode)> RegisterHealthInsuranceAsync(string studentId, string registrationPlace);
        // Lấy thông tin BHYT hiện tại của sinh viên
        Task<(bool Success, string Message, int StatusCode, HealthInsurance? Data)> GetInsuranceByStudentIdAsync(string studentId);

        Task<(bool Success, string Message, int StatusCode)> ConfirmInsurancePaymentAsync(string insuranceId);
    }
}
