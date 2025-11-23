using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using API.UnitOfWorks;
using BusinessObject.DTOs.AuthDTOs;
using BusinessObject.Entities;

using static System.Net.WebRequestMethods;

namespace API.Services.Implements
{
    public partial class AuthService : IAuthService
    {
        private readonly IAuthUow _authUow;

        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        public AuthService(IAuthUow authorUow, IConfiguration configuration, IEmailService emailService)
        {
            _authUow = authorUow;
            _configuration = configuration;
            _emailService = emailService;
        }
        public async Task<(bool Success, string Message, int StatusCode)> RegisterStudentAsync(RegisterRequest registerRequest)
        {
            // 📌 Kiểm tra account đã tồn tại theo email
            var existingAccount = await _authUow.Accounts.GetAccountByUsername(registerRequest.Email);

            if (existingAccount != null)
            {
                // 📌 Trường hợp tài khoản có rồi nhưng chưa verify email → gửi OTP mới
                if (!existingAccount.IsEmailVerified)
                {
                    // ❗ Xử lý OTP cũ (nếu có) → set IsActive = false
                    var oldOtp = await _authUow.OtpCodes.GetActiveOtp(existingAccount.UserId, "EmailVerify");
                    if (oldOtp != null)
                    {
                        oldOtp.IsActive = false;
                        _authUow.OtpCodes.UpdateOtp(oldOtp);
                    }

                    // 📌 Tạo OTP mới
                    var otp = new OtpCode
                    {
                        OtpId = Guid.NewGuid().ToString(),
                        UserId = existingAccount.UserId,
                        Code = GenerateOTP(),     // ví dụ 6 số
                        Purpose = "EmailVerify",
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                        IsActive = true
                    };

                    await _authUow.BeginTransactionAsync();
                    try
                    {
                        _authUow.OtpCodes.AddOtp(otp);
                        await _authUow.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await _authUow.RollbackAsync();
                        return (true, $"Database error during registration: {ex.Message}", 500);


                    }
                    try
                    {
                        await _emailService.SendVericationEmail(registerRequest.Email, otp.Code);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Failed to send OTP email: {ex.Message}", 500);
                    }
                    return (true, "Your email is not verified yet. A new OTP has been sent.", 200);
                }

                // 📌 Trường hợp tài khoản đã tồn tại và đã verify
                return (false, "Account with this email already exists.", 409);
            }

            // 📌 Nếu tài khoản chưa tồn tại → tạo tài khoản mới
            var newAccount = new Account
            {
                UserId = "AC-" + IdGenerator.GenerateUniqueSuffix(),
                Email = registerRequest.Email,
                Username = registerRequest.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
                Role = "Student",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsEmailVerified = false  // Chưa verify
            };

            var newStudent = new Student
            {
                StudentId = registerRequest.StudentId,
                UserId = newAccount.UserId,
                FullName = registerRequest.FullName,
                CitizenId = registerRequest.CitizenId,
                PhoneNumber = registerRequest.PhoneNumber,
                SchoolId = registerRequest.SchoolId,
                PriorityId = registerRequest.PriorityId
            };

            // 📌 Tạo OTP verify email
            var newOtp = new OtpCode
            {
                OtpId = "OTP-" + IdGenerator.GenerateUniqueSuffix(),
                UserId = newAccount.UserId,
                Code = GenerateOTP(),
                Purpose = "EmailVerify",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsActive = true
            };


            await _authUow.BeginTransactionAsync();
            try
            {
                _authUow.Accounts.AddAccount(newAccount);
                _authUow.Students.AddStudent(newStudent);
                _authUow.OtpCodes.AddOtp(newOtp);
                await _authUow.CommitAsync();
            }
            catch (Exception ex)
            {
                await _authUow.RollbackAsync();
                return (false, $"Database error during registration: {ex.Message}", 500);
            }
            try
            {
                await _emailService.SendVericationEmail(registerRequest.Email, newOtp.Code);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to send OTP email: {ex.Message}", 500);
            }
            return (true, "Account created. OTP sent to email.", 201);
        }
        public async Task<(bool Success, string Message, int StatusCode)> VerifyEmailAsync(string Otp, string UserId)
        {
            // 📌 Tìm OTP active theo code và purpose
            var otpRecord = await _authUow.OtpCodes.GetActiveOtp(Otp, "EmailVerify");
            if (otpRecord == null)
            {
                return (false, "Invalid or inactive OTP.", 400);
            }
            // 📌 Kiểm tra OTP đã hết hạn chưa
            if (otpRecord.ExpiresAt < DateTime.UtcNow)
            {
                return (false, "OTP has expired.", 400);
            }
            // 📌 Lấy tài khoản liên quan đến OTP
            if( otpRecord.UserId != UserId)
            {
                return (false, "OTP does not match the user.", 400);
            }
            var account = await _authUow.Accounts.GetAccountById(UserId);
            // 📌 Cập nhật trạng thái verify email
            account.IsEmailVerified = true;
            otpRecord.IsActive = false; // Vô hiệu hóa OTP sau khi sử dụng
            await _authUow.BeginTransactionAsync();
            try
            {
                _authUow.Accounts.UpdateAccount(account);
                _authUow.OtpCodes.UpdateOtp(otpRecord);
                await _authUow.CommitAsync();
            }
            catch (Exception ex)
            {
                await _authUow.RollbackAsync();
                return (false, $"Database error during email verification: {ex.Message}", 500);
            }
            return (true, "Email verified successfully.", 200);
        }
    }
}
