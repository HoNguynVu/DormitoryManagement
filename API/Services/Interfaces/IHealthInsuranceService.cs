namespace API.Services.Interfaces
{
    public interface IHealthInsuranceService
    {
        //  Sinh viên đăng ký mua BHYT 
        Task<(bool Success, string Message, int StatusCode)> RegisterHealthInsuranceAsync(string studentId, string registrationPlace);
    }
}
