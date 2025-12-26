using BusinessObject.DTOs.AuthDTOs;
using Microsoft.AspNetCore.Identity.Data;
using ForgotPasswordRequest = BusinessObject.DTOs.AuthDTOs.ForgotPasswordRequest;
using LoginRequest = BusinessObject.DTOs.AuthDTOs.LoginRequest;
using RegisterRequest = BusinessObject.DTOs.AuthDTOs.RegisterRequest;
using ResetPasswordRequest = BusinessObject.DTOs.AuthDTOs.ResetPasswordRequest;

namespace API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, int StatusCode)> RegisterStudentAsync(RegisterRequest registerRequest);
        Task<(bool Success, string Message, int StatusCode)> VerifyEmailAsync(VerifyEmailRequest verifyEmailRequest);
        Task<(bool Success, string Message, int StatusCode)> ResendVerificationOtpAsync(string email);
        Task<(bool Success, string Message, int StatusCode, string accessToken, string refreshToken, string userId, bool hasActiveContract, bool hasTerminatedContract)> LoginAsync(LoginRequest loginRequest);
        Task<(bool Success, string Message, int StatusCode)> ForgotPasswordAsync(ForgotPasswordRequest forgotPasswordRequest);
        Task<(bool Success, string Message, int StatusCode)> VerifyResetTokenAsync(VerifyEmailRequest verifyEmailRequest);
        Task<(bool Success, string Message, int StatusCode)> ResendResetOtpAsync(string email);
        Task<(bool Success, string Message, int StatusCode)> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest);
        Task<(bool Success, string Message, int StatusCode)> LogOut(string refreshTokenValue);
        Task<(bool Success, string Message, int StatusCode, string? AccessToken)> GetAccessToken(string refreshTokenValue);
        Task<(bool Success, string Message, int StatusCode)> RegisterManagerAsync(RegisterManagerAndAdminDTO registerRequest);
    }
}
