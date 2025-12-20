using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using BusinessObject.DTOs.AuthDTOs;


namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterStudent([FromBody] RegisterRequest registerRequest)
        {
            var result = await _authService.RegisterStudentAsync(registerRequest);
            if (result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPost("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest verifyEmailRequest)
        {
            var result = await _authService.VerifyEmailAsync(verifyEmailRequest);
            if (result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }
        
        [HttpPost("ResendOTPVerifyEmail")]
        public async Task<IActionResult> ResendOTPVerifyEmail(string email)
        {
            var result = await _authService.ResendVerificationOtpAsync(email);
            if (result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var result = await _authService.LoginAsync(loginRequest);
            if (result.Success)
            {
                return StatusCode(result.StatusCode, new
                {
                    message = result.Message,
                    accessToken = result.accessToken,
                    refreshToken = result.refreshToken,
                    userId = result.userId,
                    hasActiveContract = result.hasActiveContract
                });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest forgotPasswordRequest)
        {
            var result = await _authService.ForgotPasswordAsync(forgotPasswordRequest);
            if (result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPost("ResendOTPResetPassword")]
        public async Task<IActionResult> ResendOTPResetPassword(string email)
        {
            var result = await _authService.ResendResetOtpAsync(email);
            if (result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPost("VerifyResetToken")]
        public async Task<IActionResult> VerifyResetToken([FromBody] VerifyEmailRequest verifyEmailRequest)
        {
            var result = await _authService.VerifyResetTokenAsync(verifyEmailRequest);
            if (result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetPasswordRequest)
        {
            var result = await _authService.ResetPasswordAsync(resetPasswordRequest);
            if (result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> RefreshToken(string refreshToken)
        {
            var result = await _authService.GetAccessToken(refreshToken);
            if (result.Success)
            {
                return StatusCode(result.StatusCode, new
                {
                    message = result.Message,
                    accessToken = result.AccessToken
                });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout(string refreshToken)
        {
            var result = await _authService.LogOut(refreshToken);
            if (result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return StatusCode(result.StatusCode, new { message = result.Message });
        }
    }

}
