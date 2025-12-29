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
                        _authUow.OtpCodes.Update(oldOtp);
                    }

                    // 📌 Tạo OTP mới
                    var otp = new OtpCode
                    {
                        OtpID = "OTP-" + IdGenerator.GenerateUniqueSuffix(),
                        AccountID = existingAccount.UserId,
                        Code = GenerateOTP(),     // ví dụ 6 số
                        Purpose = "EmailVerify",
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                        IsActive = true
                    };

                    await _authUow.BeginTransactionAsync();
                    try
                    {
                        _authUow.OtpCodes.Add(otp);
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
                StudentID = registerRequest.StudentId,
                AccountID = newAccount.UserId,
                FullName = registerRequest.FullName,
                Address = registerRequest.Address,
                CitizenID = registerRequest.CitizenId,
                CitizenIDIssuePlace = registerRequest.CitizenIdIssuePlace,
                PhoneNumber = registerRequest.PhoneNumber,
                SchoolID = registerRequest.SchoolId,
                PriorityID = registerRequest.PriorityId,
                Email = registerRequest.Email,
                Gender = registerRequest.Gender
            };

            // 📌 Tạo OTP verify email
            var newOtp = new OtpCode
            {
                OtpID = "OTP-" + IdGenerator.GenerateUniqueSuffix(),
                AccountID = newAccount.UserId,
                Code = GenerateOTP(),
                Purpose = "EmailVerify",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsActive = true
            };


            await _authUow.BeginTransactionAsync();
            try
            {
                _authUow.Accounts.Add(newAccount);
                _authUow.Students.Add(newStudent);
                _authUow.OtpCodes.Add(newOtp);
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

            var account = await _authUow.Accounts.GetByIdAsync(UserId);
            // 📌 Cập nhật trạng thái verify email
            account.IsEmailVerified = true;
            otpRecord.IsActive = false; // Vô hiệu hóa OTP sau khi sử dụng
            await _authUow.BeginTransactionAsync();
            try
            {
                _authUow.Accounts.Update(account);
                _authUow.OtpCodes.Update(otpRecord);
                await _authUow.CommitAsync();
            }
            catch (Exception ex)
            {
                await _authUow.RollbackAsync();
                return (false, $"Database error during email verification: {ex.Message}", 500);
            }
            return (true, "Email verified successfully.", 200);
        }
        public async Task<(bool Success, string Message, int StatusCode)> ResendVerificationOtpAsync(string email)
        {
            var user = await _authUow.Accounts.GetAccountByUsername(email);
            if (user == null)
            {
                return (false, "Account with this email does not exist.", 404);
            }
            if (user.IsEmailVerified)
            {
                return (false, "Email is already verified.", 400);
            }
            // ❗ Xử lý OTP cũ (nếu có) → set IsActive = false
            var oldOtp = await _authUow.OtpCodes.GetActiveOtp(user.UserId, "EmailVerify");
            if (oldOtp != null)
            {
                oldOtp.IsActive = false;
                _authUow.OtpCodes.Update(oldOtp);
            }
            // 📌 Tạo OTP mới
            var otp = new OtpCode
            {
                OtpID = "OTP-" + IdGenerator.GenerateUniqueSuffix(),
                AccountID = user.UserId,
                Code = GenerateOTP(),     // ví dụ 6 số
                Purpose = "PasswordReset",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsActive = true
            };
            await _authUow.BeginTransactionAsync();
            try
            {
                _authUow.OtpCodes.Add(otp);
                await _authUow.CommitAsync();
            }
            catch (Exception ex)
            {
                await _authUow.RollbackAsync();
                return (true, $"Database error during OTP resend: {ex.Message}", 500);
            }
            try
            {
                await _emailService.SendVericationEmail(email, otp.Code);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to send OTP email: {ex.Message}", 500);
            }
            return (true, "A new OTP has been sent to your email.", 200);
        }
        public async Task<(bool Success, string Message, int StatusCode, string accessToken, string refreshToken, string userId, bool hasActiveContract, bool hasTerminatedContract, string BuildingName, string BuildingID)> LoginAsync(LoginRequest loginRequest)
        {
            var user = await _authUow.Accounts.GetAccountByUsername(loginRequest.Email);

            string buildingName = null;
            string buildingID = null;

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
                return (false, "Invalid email or password.", 401, null, null, null, false, false, null, null);
            if(user.Role =="Student")
            {
                var student = await _authUow.Students.GetStudentByEmailAsync(loginRequest.Email);
                if (student == null)
                {
                    return (false, "Student profile not found for this account.", 500, null, null, null, false, false, null, null);
                }

                if (!user.IsEmailVerified)
                {
                    await _authUow.BeginTransactionAsync();
                    try
                    {
                        _authUow.Accounts.Delete(user);
                        _authUow.Students.Delete(student);
                        await _authUow.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await _authUow.RollbackAsync();
                        return (false, $"Database error during registration: {ex.Message}", 500, null, null, null, false, false, null, null);

                    }

                    return (false, "Your email is not verified yet. Please register again", 401, null, null, null, false, false, null, null);
                }
                var accessToken = GenerateJwtToken(user);
                RefreshToken refreshToken = CreateRefreshToken(user.UserId);
                if (string.IsNullOrEmpty(user.UserId))
                    throw new InvalidOperationException("UserId is null or empty.");
                await _authUow.BeginTransactionAsync();
                try
                {
                    _authUow.RefreshTokens.RevokeRefreshToken(user.UserId);
                    _authUow.RefreshTokens.Add(refreshToken);
                    await _authUow.CommitAsync();
                }
                catch (Exception ex)
                {
                    await _authUow.RollbackAsync();
                    return (false, $"Database error during login: {ex.Message}", 500, null, null, null, false, false, null, null);
                }
                bool hasActiveContract = await _authUow.Contracts.GetActiveAndNearExpiringContractByStudentId(student.StudentID) != null;
                var lastContract = await _authUow.Contracts.GetLastContractByStudentIdAsync(student.StudentID);
                bool hasTerminatedContract = lastContract != null && lastContract.ContractStatus == "Terminated";
                var message = $"Welcome back, {student.FullName}!";
                return (true, message, 200, accessToken, refreshToken.Token, user.UserId, hasActiveContract, hasTerminatedContract, null, null);
            }

            else
            {
                if (user.Role == "Manager")
                {
                    // Giả sử bạn có Repository BuildingManagers và Buildings trong UnitOfWork
                    // Nếu dùng DbContext trực tiếp thì thay _authUow.Context...

                    // Tìm Profile Manager theo AccountID
                    var managerProfile = await _authUow.BuildingManagers
                        .FirstOrDefaultAsync(m => m.AccountID == user.UserId);

                    if (managerProfile != null)
                    {
                        // Tìm Tòa nhà theo ManagerID
                        var building = await _authUow.Buildings
                            .FirstOrDefaultAsync(b => b.ManagerID == managerProfile.ManagerID);

                        if (building != null)
                        {
                            buildingName = building.BuildingName; 
                            buildingID = building.BuildingID;
                        }
                    }
                }
                var accessToken = GenerateJwtToken(user);
                RefreshToken refreshToken = CreateRefreshToken(user.UserId);
                if (string.IsNullOrEmpty(user.UserId))
                    throw new InvalidOperationException("UserId is null or empty.");
                await _authUow.BeginTransactionAsync();
                try
                {
                    _authUow.RefreshTokens.RevokeRefreshToken(user.UserId);
                    _authUow.RefreshTokens.Add(refreshToken);
                    await _authUow.CommitAsync();
                }
                catch (Exception ex)
                {
                    await _authUow.RollbackAsync();
                    return (false, $"Database error during login: {ex.Message}", 500, null, null, null, false, false, null, null);
                }
                var message = $"Welcome back!";
                return (true, message, 200, accessToken, refreshToken.Token, user.UserId, false, false, buildingName, buildingID);
            }
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
                OtpID = "OTP-" + IdGenerator.GenerateUniqueSuffix(),
                AccountID = user.UserId,
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
                    _authUow.OtpCodes.Update(oldOtp);
                }
                _authUow.OtpCodes.Add(otp);
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
            return (true, "OTP verified successfully.", 200);
        }
        public async Task<(bool Success, string Message, int StatusCode)> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest)
        {
            var user = await _authUow.Accounts.GetAccountByUsername(resetPasswordRequest.email);
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
            if (otpRecord.Code != resetPasswordRequest.otp)
            {
                return (false, "Incorrect OTP.", 400);
            }
            otpRecord.IsActive = false;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordRequest.password);
            await _authUow.BeginTransactionAsync();
            try
            {
                _authUow.OtpCodes.Update(otpRecord);
                _authUow.Accounts.Update(user);
                await _authUow.CommitAsync();
            }
            catch (Exception ex)
            {
                await _authUow.RollbackAsync();
                return (false, $"Database error during password reset: {ex.Message}", 500);
            }
            return (true, "Password has been reset successfully.", 200);
        }
        public async Task<(bool Success, string Message, int StatusCode)> ResendResetOtpAsync(string email)
        {
            var user = await _authUow.Accounts.GetAccountByUsername(email);
            if (user == null)
            {
                return (false, "Account with this email does not exist.", 404);
            }
            if (user.IsEmailVerified)
            {
                return (false, "Email is already verified.", 400);
            }
            // ❗ Xử lý OTP cũ (nếu có) → set IsActive = false
            var oldOtp = await _authUow.OtpCodes.GetActiveOtp(user.UserId, "EmailVerify");
            if (oldOtp != null)
            {
                oldOtp.IsActive = false;
                _authUow.OtpCodes.Update(oldOtp);
            }
            // 📌 Tạo OTP mới
            var otp = new OtpCode
            {
                OtpID = "OTP-" + IdGenerator.GenerateUniqueSuffix(),
                AccountID = user.UserId,
                Code = GenerateOTP(),     // ví dụ 6 số
                Purpose = "EmailVerify",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsActive = true
            };
            await _authUow.BeginTransactionAsync();
            try
            {
                _authUow.OtpCodes.Add(otp);
                await _authUow.CommitAsync();
            }
            catch (Exception ex)
            {
                await _authUow.RollbackAsync();
                return (true, $"Database error during OTP resend: {ex.Message}", 500);
            }
            try
            {
                await _emailService.SendVericationEmail(email, otp.Code);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to send OTP email: {ex.Message}", 500);
            }
            return (true, "A new OTP has been sent to your email.", 200);
        }
        public async Task<(bool Success, string Message, int StatusCode)> LogOut(string refreshTokenValue)
        {
            var refreshToken = await _authUow.RefreshTokens.GetRefreshToken(refreshTokenValue);
            if (refreshToken == null || refreshToken.RevokedAt != null)
            {
                return (false, "Invalid or inactive refresh token.", 400);
            }
            refreshToken.RevokedAt = DateTime.UtcNow;
            await _authUow.BeginTransactionAsync();
            try
            {
                _authUow.RefreshTokens.Update(refreshToken);
                await _authUow.CommitAsync();
            }
            catch (Exception ex)
            {
                await _authUow.RollbackAsync();
                return (false, $"Database error during logout: {ex.Message}", 500);
            }
            return (true, "Logged out successfully.", 200);
        }
        public async Task<(bool Success, string Message, int StatusCode, string? AccessToken)> GetAccessToken(string refreshTokenValue)
        {
            var refreshToken = await _authUow.RefreshTokens.GetRefreshToken(refreshTokenValue);
            if (refreshToken == null || refreshToken.RevokedAt != null || refreshToken.ExpiresAt <= DateTime.UtcNow)
            {
                return (false, "Invalid or expired refresh token.", 400, null);
            }
            var user = await _authUow.Accounts.GetByIdAsync(refreshToken.AccountID);
            if (user == null)
            {
                return (false, "Account associated with the refresh token does not exist.", 404, null);
            }
            var newAccessToken = GenerateJwtToken(user);
            return (true, "Access token generated successfully.", 200, newAccessToken);
        }
        public async Task<(bool Success, string Message, int StatusCode)> RegisterManagerAsync(RegisterManagerAndAdminDTO registerRequest)
        {
            // 📌 Kiểm tra account đã tồn tại theo email
            var existingAccount = await _authUow.Accounts.GetAccountByUsername(registerRequest.email);
            if (existingAccount != null)
            {
                return (false, "Account with this email already exists.", 409);
            }
            // 📌 Nếu tài khoản chưa tồn tại → tạo tài khoản mới
            var newAccount = new Account
            {
                UserId = "AC-" + IdGenerator.GenerateUniqueSuffix(),
                Email = registerRequest.email,
                Username = registerRequest.email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.password),
                Role = "Manager",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsEmailVerified = true  // Mặc định đã verify
            };
            await _authUow.BeginTransactionAsync();
            try
            {
                _authUow.Accounts.Add(newAccount);
                await _authUow.CommitAsync();
            }
            catch (Exception ex)
            {
                await _authUow.RollbackAsync();
                return (false, $"Database error during registration: {ex.Message}", 500);
            }
            return (true, "Manager account created successfully.", 201);
        }
    }
}
