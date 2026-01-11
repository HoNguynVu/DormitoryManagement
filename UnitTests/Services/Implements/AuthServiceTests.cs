using API.Services.Implements;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.AuthDTOs;
using BusinessObject.Entities;
using DataAccess.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Services.Implements
{
    public class AuthServiceTests
    {
        private readonly Mock<IAuthUow> _mockUow;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<IEmailService> _mockEmailService;

        private readonly Mock<IAccountRepository> _mockAccountRepo;
        private readonly Mock<IStudentRepository> _mockStudentRepo;
        private readonly Mock<IOtpRepository> _mockOtpRepo;
        private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepo;
        private readonly Mock<IContractRepository> _mockContractRepo;
        private readonly Mock<IBuildingManagerRepository> _mockManagerRepo;
        private readonly Mock<IBuildingRepository> _mockBuildingRepo;

        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Khởi tạo Mock
            _mockUow = new Mock<IAuthUow>();
            _mockConfig = new Mock<IConfiguration>();
            _mockEmailService = new Mock<IEmailService>();

            _mockAccountRepo = new Mock<IAccountRepository>();
            _mockStudentRepo = new Mock<IStudentRepository>();
            _mockOtpRepo = new Mock<IOtpRepository>();
            _mockRefreshTokenRepo = new Mock<IRefreshTokenRepository>();
            _mockContractRepo = new Mock<IContractRepository>();
            _mockManagerRepo = new Mock<IBuildingManagerRepository>();
            _mockBuildingRepo = new Mock<IBuildingRepository>();

            // Link Mock Repo vào Mock UoW
            _mockUow.Setup(u => u.Accounts).Returns(_mockAccountRepo.Object);
            _mockUow.Setup(u => u.Students).Returns(_mockStudentRepo.Object);
            _mockUow.Setup(u => u.OtpCodes).Returns(_mockOtpRepo.Object);
            _mockUow.Setup(u => u.RefreshTokens).Returns(_mockRefreshTokenRepo.Object);
            _mockUow.Setup(u => u.Contracts).Returns(_mockContractRepo.Object);
            _mockUow.Setup(u => u.BuildingManagers).Returns(_mockManagerRepo.Object);
            _mockUow.Setup(u => u.Buildings).Returns(_mockBuildingRepo.Object);

            // Giả lập Transaction (Để không bị lỗi NullReference khi service gọi Begin/Commit)
            _mockUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

            // Giả lập Config cho JWT
            var mockSectionToken = new Mock<IConfigurationSection>();
            mockSectionToken.Setup(s => s.Value).Returns("DayLaMotCaiKeyRatDai_RatDai_RatDai_PhaiDu64KyTu_DeKhongBiLoi_HmacSha512_1234567890");

            var mockSectionIssuer = new Mock<IConfigurationSection>();
            mockSectionIssuer.Setup(s => s.Value).Returns("TestIssuer");

            var mockSectionAudience = new Mock<IConfigurationSection>();
            mockSectionAudience.Setup(s => s.Value).Returns("TestAudience");

            // Setup hành vi: Khi code gọi GetSection("Key") thì trả về section giả ở trên
            _mockConfig.Setup(c => c.GetSection("AppSettings:Token")).Returns(mockSectionToken.Object);
            _mockConfig.Setup(c => c.GetSection("AppSettings:Issuer")).Returns(mockSectionIssuer.Object);
            _mockConfig.Setup(c => c.GetSection("AppSettings:Audience")).Returns(mockSectionAudience.Object);


            // Inject vào Service
            _authService = new AuthService(_mockUow.Object, _mockConfig.Object, _mockEmailService.Object);
        }

        // ==========================================
        //  REGISTER 
        // ==========================================

        [Fact(DisplayName = "Register: Thành công khi tạo tài khoản mới")]
        public async Task RegisterStudent_ShouldSuccess_WhenUserIsNew()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "new@fpt.edu.vn",
                Password = "123",
                FullName = "New User",
                StudentId = "SE1"
            };

            // Setup: Không tìm thấy account cũ
            _mockAccountRepo.Setup(r => r.GetAccountByUsername(request.Email)).ReturnsAsync((Account)null);

            // Act
            var result = await _authService.RegisterStudentAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal("Account created. OTP sent to email.", result.Message);

            // Verify: Đã gọi hàm Add cho Account, Student và OTP
            _mockAccountRepo.Verify(r => r.Add(It.IsAny<Account>()), Times.Once);
            _mockStudentRepo.Verify(r => r.Add(It.IsAny<Student>()), Times.Once);
            _mockOtpRepo.Verify(r => r.Add(It.IsAny<OtpCode>()), Times.Once);
            _mockEmailService.Verify(e => e.SendVericationEmail(request.Email, It.IsAny<string>()), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "Register: Thất bại khi Email đã tồn tại và đã Verify")]
        public async Task RegisterStudent_ShouldFail_WhenEmailAlreadyVerified()
        {
            // Arrange
            var request = new RegisterRequest { Email = "exist@fpt.edu.vn" };
            var existingAcc = new Account { Email = request.Email, IsEmailVerified = true }; // Đã verify

            _mockAccountRepo.Setup(r => r.GetAccountByUsername(request.Email)).ReturnsAsync(existingAcc);

            // Act
            var result = await _authService.RegisterStudentAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(409, result.StatusCode);
            _mockAccountRepo.Verify(r => r.Add(It.IsAny<Account>()), Times.Never); // Không được add mới
        }

        [Fact(DisplayName = "Register: Gửi lại OTP nếu tài khoản tồn tại nhưng CHƯA verify")]
        public async Task RegisterStudent_ShouldResendOTP_WhenEmailExistsButNotVerified()
        {
            // Arrange
            var request = new RegisterRequest { Email = "notverified@fpt.edu.vn" };
            var existingAcc = new Account { UserId = "U1", Email = request.Email, IsEmailVerified = false }; // Chưa verify

            var oldOtp = new OtpCode { OtpID = "OldOTP", IsActive = true };

            _mockAccountRepo.Setup(r => r.GetAccountByUsername(request.Email)).ReturnsAsync(existingAcc);
            _mockOtpRepo.Setup(r => r.GetActiveOtp("U1", "EmailVerify")).ReturnsAsync(oldOtp);

            // Act
            var result = await _authService.RegisterStudentAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode); // Code bạn trả về 200 đoạn này
            Assert.Contains("new OTP has been sent", result.Message);

            // Verify logic: OTP cũ phải bị vô hiệu hóa, OTP mới được thêm vào
            Assert.False(oldOtp.IsActive);
            _mockOtpRepo.Verify(r => r.Update(oldOtp), Times.Once);
            _mockOtpRepo.Verify(r => r.Add(It.IsAny<OtpCode>()), Times.Once);
        }

        [Fact(DisplayName = "Register: Trả về 500 khi DB lỗi")]
        public async Task RegisterStudent_ShouldReturn500_WhenDbTransactionFails()
        {
            // Arrange
            var request = new RegisterRequest { Email = "fail@fpt.edu.vn", Password = "123" };
            _mockAccountRepo.Setup(r => r.GetAccountByUsername(request.Email)).ReturnsAsync((Account)null);

            // Giả lập: Commit bị lỗi
            _mockUow.Setup(u => u.CommitAsync()).ThrowsAsync(new Exception("DB connection lost"));

            // Act
            var result = await _authService.RegisterStudentAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            _mockUow.Verify(u => u.RollbackAsync(), Times.Once); // Phải gọi rollback
        }

        // ==========================================
        // LOGIN 
        // ==========================================

        [Fact(DisplayName = "Login: Thành công (Student)")]
        public async Task Login_ShouldSuccess_WhenStudentCredentialsCorrect()
        {
            // Arrange
            var request = new LoginRequest { Email = "student@test.com", Password = "Pass123" };

            // Tạo hash thật để verify
            string hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var user = new Account { 
                UserId = "U1",
                Username = "student@test.com",
                Email = request.Email, 
                PasswordHash = hash,
                Role = "Student", 
                IsEmailVerified = true 
            };
            var student = new Student 
            { 
                StudentID = "S1", 
                AccountID = "U1", 
                FullName = "Test"
            };

            _mockAccountRepo.Setup(r => r.GetAccountByUsername(request.Email)).ReturnsAsync(user);
            _mockStudentRepo.Setup(r => r.GetStudentByEmailAsync(request.Email)).ReturnsAsync(student);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.accessToken);
            Assert.NotNull(result.refreshToken);

            // Verify Refresh Token logic
            _mockUow.Verify(u => u.RefreshTokens.RevokeRefreshToken("U1"), Times.Once);
            _mockUow.Verify(u => u.RefreshTokens.Add(It.IsAny<RefreshToken>()), Times.Once);
        }

        // Gom nhóm các case login thất bại do sai thông tin
        [Theory(DisplayName = "Login: Thất bại do sai Email hoặc Password")]
        [InlineData("wrong@email.com", "anyPass", "UserNotFound")] // Case 1: Sai Email (User null)
        [InlineData("right@email.com", "wrongPass", "WrongPassword")] // Case 2: Đúng Email, Sai Pass
        public async Task Login_ShouldFail_WhenCredentialsInvalid(string email, string pass, string scenario)
        {
            // Arrange
            Account userReturn = null;
            if (scenario == "WrongPassword")
            {
                // Nếu test case sai pass, trả về user có hash password khác
                userReturn = new Account
                {
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("RealPass"),
                    Role = "Student"
                };
            }

            _mockAccountRepo.Setup(r => r.GetAccountByUsername(email)).ReturnsAsync(userReturn);

            // Act
            var result = await _authService.LoginAsync(new LoginRequest { Email = email, Password = pass });

            // Assert
            Assert.False(result.Success);
            Assert.Equal(401, result.StatusCode);
            Assert.Equal("Invalid email or password.", result.Message);
        }

        [Fact(DisplayName = "Login: Thất bại và XÓA Account nếu chưa Verify")]
        public async Task Login_ShouldFailAndRemove_WhenStudentNotVerified()
        {
            // Arrange
            var request = new LoginRequest { Email = "unverified@test.com", Password = "Pass" };
            var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new Account { UserId = "U1", PasswordHash = hash, Role = "Student", IsEmailVerified = false }; // False
            var student = new Student { StudentID = "S1", AccountID = "U1" };

            _mockAccountRepo.Setup(r => r.GetAccountByUsername(request.Email)).ReturnsAsync(user);
            _mockStudentRepo.Setup(r => r.GetStudentByEmailAsync(request.Email)).ReturnsAsync(student);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(401, result.StatusCode);

            // Verify logic cực đoan của bạn: Delete Account và Student
            _mockUow.Verify(u => u.Accounts.Delete(user), Times.Once);
            _mockUow.Verify(u => u.Students.Delete(student), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        // ==========================================
        // FORGOT PASSWORD
        // ==========================================

        [Fact(DisplayName = "ForgotPassword: Thành công (Gửi OTP qua email)")]
        public async Task ForgotPassword_ShouldSuccess_WhenEmailExists()
        {
            // Arrange
            string email = "student@test.com";
            var forgot = new ForgotPasswordRequest { email = email };
            var user = new Account { UserId = "U1", Email = email, IsEmailVerified = true };

            // Mock: Tìm thấy user
            _mockAccountRepo.Setup(r => r.GetAccountByUsername(email)).ReturnsAsync(user);

            // Act
            var result = await _authService.ForgotPasswordAsync(forgot);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Password reset OTP sent to email.", result.Message);

            // Verify:
            // 1. Phải tạo OTP mới
            _mockOtpRepo.Verify(r => r.Add(It.IsAny<OtpCode>()), Times.Once);
            // 2. Phải lưu DB
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "ForgotPassword: Thất bại khi Email không tồn tại")]
        public async Task ForgotPassword_ShouldFail_WhenUserNotFound()
        {
            // Arrange
            string email = "ghost@test.com";
            var forgot = new ForgotPasswordRequest { email = email };
            _mockAccountRepo.Setup(r => r.GetAccountByUsername(email)).ReturnsAsync((Account)null);

            // Act
            var result = await _authService.ForgotPasswordAsync(forgot);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Account with this email does not exist.", result.Message);

            // Verify: Không được gửi mail hay lưu DB
            _mockEmailService.Verify(e => e.SendVericationEmail(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockUow.Verify(u => u.CommitAsync(), Times.Never);
        }
        // ==========================================
        // VERIFY EMAIL
        // ==========================================

        [Fact(DisplayName = "VerifyEmail: Thành công")]
        public async Task VerifyEmail_ShouldSuccess_WhenOtpValid()
        {
            // Arrange
            var request = new VerifyEmailRequest { Email = "user@test.com", OTP = "123456" };
            var user = new Account { UserId = "U1", Email = request.Email, IsEmailVerified = false };
            var otp = new OtpCode
            {
                Code = "123456",
                ExpiresAt = DateTime.UtcNow.AddMinutes(5), // Còn hạn
                IsActive = true
            };

            _mockAccountRepo.Setup(r => r.GetAccountByUsername(request.Email)).ReturnsAsync(user);
            _mockAccountRepo.Setup(r => r.GetByIdAsync("U1")).ReturnsAsync(user);
            _mockOtpRepo.Setup(r => r.GetActiveOtp("U1", "EmailVerify")).ReturnsAsync(otp);

            // Act
            var result = await _authService.VerifyEmailAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.True(user.IsEmailVerified); // User phải được set true
            Assert.False(otp.IsActive);        // OTP phải bị vô hiệu hóa

            _mockUow.Verify(u => u.Accounts.Update(user), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "VerifyEmail: Thất bại do OTP hết hạn")]
        public async Task VerifyEmail_ShouldFail_WhenOtpExpired()
        {
            // Arrange
            var request = new VerifyEmailRequest { Email = "user@test.com" };
            var user = new Account { UserId = "U1" };
            var otp = new OtpCode { ExpiresAt = DateTime.UtcNow.AddMinutes(-1) }; // Hết hạn

            _mockAccountRepo.Setup(r => r.GetAccountByUsername(request.Email)).ReturnsAsync(user);
            _mockOtpRepo.Setup(r => r.GetActiveOtp("U1", "EmailVerify")).ReturnsAsync(otp);

            // Act
            var result = await _authService.VerifyEmailAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("OTP has expired.", result.Message);
            _mockUow.Verify(u => u.CommitAsync(), Times.Never);
        }

        // ==========================================
        // RESET PASSWORD
        // ==========================================

        [Fact(DisplayName = "ResetPassword: Thành công")]
        public async Task ResetPassword_ShouldSuccess_WhenOtpValid()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                email = "user@test.com",
                otp = "123456",
                password = "NewPassword123"
            };

            var user = new Account { UserId = "U1", Email = request.email };
            var otp = new OtpCode
            {
                Code = "123456",
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsActive = true
            };

            // Mock setup
            _mockAccountRepo.Setup(r => r.GetAccountByUsername(request.email)).ReturnsAsync(user);
            _mockOtpRepo.Setup(r => r.GetActiveOtp("U1", "PasswordReset")).ReturnsAsync(otp);

            // Act
            var result = await _authService.ResetPasswordAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);

            // Password đã được đổi chưa?
            _mockAccountRepo.Verify(r => r.Update(user), Times.Once);

            Assert.False(otp.IsActive);
            _mockOtpRepo.Verify(r => r.Update(otp), Times.Once);

            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "ResetPassword: Thất bại khi OTP sai hoặc không khớp")]
        public async Task ResetPassword_ShouldFail_WhenOtpIncorrect()
        {
            // Arrange
            var request = new ResetPasswordRequest { email = "user@test.com", otp = "WRONG_CODE" };
            var user = new Account { UserId = "U1" };

            // Mock trả về OTP thật là 123456
            var realOtp = new OtpCode { Code = "123456", IsActive = true, ExpiresAt = DateTime.UtcNow.AddMinutes(5) };

            _mockAccountRepo.Setup(r => r.GetAccountByUsername(request.email)).ReturnsAsync(user);
            _mockOtpRepo.Setup(r => r.GetActiveOtp("U1", "PasswordReset")).ReturnsAsync(realOtp);

            // Act
            var result = await _authService.ResetPasswordAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Incorrect OTP.", result.Message); // Message tùy code bạn

            // Verify: Không được update user
            _mockAccountRepo.Verify(r => r.Update(It.IsAny<Account>()), Times.Never);
        }

        [Fact(DisplayName = "ResetPassword: Thất bại khi OTP hết hạn")]
        public async Task ResetPassword_ShouldFail_WhenOtpExpired()
        {
            // Arrange
            var request = new ResetPasswordRequest { email = "user@test.com", otp = "123456" };
            var user = new Account { UserId = "U1" };

            var expiredOtp = new OtpCode
            {
                Code = "123456",
                IsActive = true,
                ExpiresAt = DateTime.UtcNow.AddMinutes(-5) // Đã hết hạn 5 phút trước
            };

            _mockAccountRepo.Setup(r => r.GetAccountByUsername(request.email)).ReturnsAsync(user);
            _mockOtpRepo.Setup(r => r.GetActiveOtp("U1", "PasswordReset")).ReturnsAsync(expiredOtp);

            // Act
            var result = await _authService.ResetPasswordAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("expired", result.Message); // Check message có chứa chữ expired
            _mockUow.Verify(u => u.CommitAsync(), Times.Never);
        }
    }
}
