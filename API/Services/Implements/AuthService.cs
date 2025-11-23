using API.Services.Helpers;
using API.Services.Interfaces;
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
                        OtpId = "OTP-" + IdGenerator.GenerateUniqueSuffix(),
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
                PriorityId = registerRequest.PriorityId,
                Email = registerRequest.Email
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
        public async Task<(bool Success, string Message, int StatusCode)> VerifyEmailAsync(VerifyEmailRequest request)
        {
            // 📌 Tìm OTP active theo code và purpose
            var UserId = (await _authUow.Accounts.GetAccountByUsername(request.Email))?.UserId;
            var otpRecord = await _authUow.OtpCodes.GetActiveOtp(UserId, "EmailVerify");
            
            if (otpRecord == null)
            {
                return (false, "Invalid or inactive OTP.", 400);
            }
            // 📌 Kiểm tra OTP đã hết hạn chưa
            if (otpRecord.ExpiresAt < DateTime.UtcNow)
            {
                return (false, "OTP has expired.", 400);
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
        public async Task<(bool Success, string Message, int StatusCode, string accessToken, string refreshToken, string userId)> LoginAsync(LoginRequest loginRequest)
        {
            var user = await _authUow.Accounts.GetAccountByUsername(loginRequest.Email);
            
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
                return (false, "Invalid email or password.", 401, null, null, null);
            var student = await _authUow.Students.GetStudentByEmailAsync(loginRequest.Email);
            if (student == null)
            {
                return (false, "Student profile not found for this account.", 500, null, null, null);
            }

            if (!user.IsEmailVerified)
            {
                await _authUow.BeginTransactionAsync();
                try
                {
                    _authUow.Accounts.DeleteAccount(user);
                    _authUow.Students.DeleteStudent(student);
                    await _authUow.CommitAsync();
                }
                catch (Exception ex)
                {
                    await _authUow.RollbackAsync();
                    return (false, $"Database error during registration: {ex.Message}", 500, null, null, null);

                }
                
                return (false, "Your email is not verified yet. Please register again", 401, null, null, null);
            }  
            var accessToken = GenerateJwtToken(user);
            RefreshToken refreshToken = CreateRefreshToken(user.UserId);
            if (string.IsNullOrEmpty(user.UserId))
                throw new InvalidOperationException("UserId is null or empty.");
            await _authUow.BeginTransactionAsync();
            try
            {
                _authUow.RefreshTokens.RevokeRefreshToken(user.UserId);
                _authUow.RefreshTokens.AddRefreshToken(refreshToken);
                await _authUow.CommitAsync();
            }
            catch (Exception ex)
            {
                await _authUow.RollbackAsync();
                return (false, $"Database error during login: {ex.Message}", 500, null, null, null);
            }
           
            var message = $"Welcome back, {student.FullName}!";
            return (true, message, 200, accessToken, refreshToken.Token, user.UserId);
        }
        public async Task<(bool Success, string Message, int StatusCode)> ForgotPasswordAsync(ForgotPasswordRequest forgotPasswordRequest)
        {
            var user = await _authUow.Accounts.GetAccountByUsername(forgotPasswordRequest.email);
            if (user == null)
            {
                return (false, "Account with this email does not exist.", 404);
            }
            if (!user.IsEmailVerified)
            {
                return (false, "Email is not verified. Cannot reset password.", 400);
            }
            var otp = new OtpCode
            {
                OtpId = "OTP-" + IdGenerator.GenerateUniqueSuffix(),
                UserId = user.UserId,
                Code = GenerateOTP(),
                Purpose = "PasswordReset",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsActive = true
            };
            
            await _authUow.BeginTransactionAsync();
            try
            {
                var oldOtp = await _authUow.OtpCodes.GetActiveOtp(user.UserId, "PasswordReset");
                if (oldOtp != null)
                {
                    oldOtp.IsActive = false;
                    _authUow.OtpCodes.UpdateOtp(oldOtp);
                }
                _authUow.OtpCodes.AddOtp(otp);
                await _authUow.CommitAsync();
            }
            catch (Exception ex)
            {
                await _authUow.RollbackAsync();
                return (false, $"Database error during password reset request: {ex.Message}", 500);
            }
            try
            {
                await _emailService.SendResetPasswordEmail(forgotPasswordRequest.email, otp.Code);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to send password reset email: {ex.Message}", 500);
            }
            return (true, "Password reset OTP sent to email.", 200);

        }
        public async Task<(bool Success, string Message, int StatusCode)> VerifyResetTokenAsync(VerifyEmailRequest verifyEmailRequest)
        {
            var user = await _authUow.Accounts.GetAccountByUsername(verifyEmailRequest.Email);
            if (user == null)
            {
                return (false, "Account with this email does not exist.", 404);
            }
            var otpRecord = await _authUow.OtpCodes.GetActiveOtp(user.UserId, "PasswordReset");
            if (otpRecord == null)
            {
                return (false, "Invalid or inactive OTP.", 400);
            }
            if (otpRecord.ExpiresAt < DateTime.UtcNow)
            {
                return (false, "OTP has expired.", 400);
            }
            if (otpRecord.Code != verifyEmailRequest.OTP)
            {
                return (false, "Incorrect OTP.", 400);
            }
            otpRecord.IsActive = false; // Vô hiệu hóa OTP sau khi sử dụng
            await _authUow.BeginTransactionAsync();
            try
            {
                _authUow.OtpCodes.UpdateOtp(otpRecord);
                await _authUow.CommitAsync();
            }
            catch (Exception ex)
            {
                await _authUow.RollbackAsync();
                return (false, $"Database error during email verification: {ex.Message}", 500);
            }
            return (true, "OTP verified successfully.", 200);
        }
        public async Task<(bool Success, string Message, int StatusCode)> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest)
        {
            var user = await _authUow.Accounts.GetAccountByUsername(resetPasswordRequest.email);
            if (user == null)
            {
                return (false, "Account with this email does not exist.", 404);
            }
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordRequest.password);
            await _authUow.BeginTransactionAsync();
            try
            {
                _authUow.Accounts.UpdateAccount(user);
                await _authUow.CommitAsync();
            }
            catch (Exception ex)
            {
                await _authUow.RollbackAsync();
                return (false, $"Database error during password reset: {ex.Message}", 500);
            }
            return (true, "Password has been reset successfully.", 200);
        }
    }
}
