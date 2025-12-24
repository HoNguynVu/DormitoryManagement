using BusinessObject.DTOs.HealthInsuranceDTOs;
using BusinessObject.Entities;
using DocumentFormat.OpenXml;

namespace API.Services.Interfaces
{
    public interface IHealthInsuranceService
    {
        //  Sinh viên đăng ký mua BHYT 
        Task<(bool Success, string Message, int StatusCode,string? insuranceId)> RegisterHealthInsuranceAsync(string studentId, string hospitalId, string cardNumber);
        // Lấy thông tin BHYT hiện tại của sinh viên
        Task<(bool Success, string Message, int StatusCode, HealthInsurance? Data)> GetInsuranceByStudentIdAsync(string studentId);

        Task<(bool Success, string Message, int StatusCode)> ConfirmInsurancePaymentAsync(string insuranceId);

        Task<(bool Success, string Message, int StatusCode, string? healthPriceId)> CreateHealthInsurancePriceAsync(CreateHealthPriceDTO request);

        // Get
        Task<(bool Success, string Message,int StatusCode,HealthDetailDto dto)> GetDetailHealth(string insuranceId);
        Task<(bool Success, string Message, int StatusCode,IEnumerable<SummaryHealthDto> dto)> GetHealthInsuranceFiltered(string? keyword,string? hospitalName,int? year,string? status);
    }
}
