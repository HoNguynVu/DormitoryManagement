using BusinessObject.DTOs.AuthDTOs;
using Microsoft.AspNetCore.Identity.Data;
using RegisterRequest = BusinessObject.DTOs.AuthDTOs.RegisterRequest;

namespace API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, int StatusCode)> RegisterStudentAsync(RegisterRequest registerRequest);
        Task<(bool Success, string Message, int StatusCode)> VerifyEmailAsync(string Otp, string UserId);
    }
}
