using API.IntegrationTests.Factories;
using API.Services.Interfaces;
using BusinessObject.DTOs.AuthDTOs;
using BusinessObject.Entities;
using DataAccess.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;
using BCrypt.Net;

namespace API.IntegrationTests.Services
{
    [Collection("Integration Tests")]
    public class AuthServiceTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public AuthServiceTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact(DisplayName = "Đăng ký: Tạo Account và Student thành công")]
        public async Task RegisterStudentAsync_ShouldCreateAccountAndStudent_WhenDataIsValid()
        {
            // 1. Arrange
            using var scope = _factory.Services.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var context = scope.ServiceProvider.GetRequiredService<DormitoryDbContext>();

            var uniqueSuffix = Guid.NewGuid().ToString().Substring(0, 8);
            var request = new RegisterRequest
            {
                Email = $"student_{uniqueSuffix}@test.com",
                Password = "Password123!",
                FullName = "Nguyen Van A",
                StudentId = $"SV_{uniqueSuffix}",
                PhoneNumber = "0987654321",
                Address = "HCM",
                CitizenId = $"CCCD_{uniqueSuffix}",
                SchoolId = "SCH01",
                Gender = "Male"
            };

            // 2. Act
            var result = await authService.RegisterStudentAsync(request);

            // 3. Assert
            Assert.True(result.Success, result.Message);
            Assert.Equal(201, result.StatusCode);

            var createdAccount = await context.Accounts.FirstOrDefaultAsync(a => a.Email == request.Email);
            Assert.NotNull(createdAccount);
            Assert.Equal("Student", createdAccount.Role);
        }

        [Fact(DisplayName = "Xác thực Email: Thành công khi OTP đúng")]
        public async Task VerifyEmailAsync_ShouldVerifyAccount_WhenOtpIsCorrect()
        {
            using var scope = _factory.Services.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var context = scope.ServiceProvider.GetRequiredService<DormitoryDbContext>();

            var userId = Guid.NewGuid().ToString();
            var email = $"verify_{userId}@test.com";

            // Seed Data
            context.Accounts.Add(new Account
            {
                UserId = userId,
                Email = email,
                Username = email,
                PasswordHash = "hashed_pass",
                IsEmailVerified = false,
                Role = "Student",
                IsActive = true
            });

            context.OtpCodes.Add(new OtpCode
            {
                OtpID = Guid.NewGuid().ToString(),
                AccountID = userId,
                Code = "123456",
                Purpose = "EmailVerify",
                IsActive = true,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            // Act
            var result = await authService.VerifyEmailAsync(new VerifyEmailRequest { Email = email, OTP = "123456" });

            // Assert
            Assert.True(result.Success);

            // Clear tracking để đảm bảo lấy data mới nhất từ DB giả lập
            context.ChangeTracker.Clear();
            var updatedAccount = await context.Accounts.FindAsync(userId);
            Assert.True(updatedAccount.IsEmailVerified);
        }

        [Fact(DisplayName = "Đăng nhập: Trả về Token khi đúng thông tin")]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreCorrect()
        {
            using var scope = _factory.Services.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var context = scope.ServiceProvider.GetRequiredService<DormitoryDbContext>();

            var rawPass = "Password123";
            var userId = Guid.NewGuid().ToString();
            var email = $"login_{userId}@test.com";

            // Seed Data
            context.Accounts.Add(new Account
            {
                UserId = userId,
                Email = email,
                Username = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(rawPass),
                Role = "Student",
                IsEmailVerified = true,
                IsActive = true
            });

            // Seed Student info (vì login có thể cần join bảng Student)
            context.Students.Add(new Student
            {
                StudentID = $"SV_{userId}",
                AccountID = userId,
                FullName = "Test Login",
                Email = email,
                Gender = "Male",
                CitizenID = "123",
                SchoolID = "SCH"
            });
            await context.SaveChangesAsync();

            // Act
            var result = await authService.LoginAsync(new LoginRequest { Email = email, Password = rawPass });

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.accessToken);
            Assert.Equal(userId, result.userId);
        }

        [Fact(DisplayName = "Đăng nhập: Thất bại khi sai mật khẩu")]
        public async Task LoginAsync_ShouldFail_WhenPasswordIsWrong()
        {
            using var scope = _factory.Services.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var context = scope.ServiceProvider.GetRequiredService<DormitoryDbContext>();

            var email = $"wrong_{Guid.NewGuid()}@test.com";
            context.Accounts.Add(new Account
            {
                UserId = Guid.NewGuid().ToString(),
                Email = email,
                Username = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass"),
                Role = "Student",
                IsEmailVerified = true,
                IsActive = true
            });
            await context.SaveChangesAsync();

            // Act
            var result = await authService.LoginAsync(new LoginRequest { Email = email, Password = "WrongPass" });

            // Assert
            Assert.False(result.Success);
            Assert.Equal(401, result.StatusCode);
        }
    }
}