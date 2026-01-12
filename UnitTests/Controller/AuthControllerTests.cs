using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using API.Controllers;
using API.Services.Interfaces;
using BusinessObject.DTOs.AuthDTOs;

namespace UnitTests.Controller
{ 
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(_mockAuthService.Object);
        }

        [Fact(DisplayName = "Đăng ký sinh viên thành công trả về mã 201")]
        public async Task RegisterStudent_Returns201_WhenSuccess()
        {
            // Arrange
            var request = new RegisterRequest
            {
                StudentId = "SE123456",
                Email = "student@fpt.edu.vn",
                Password = "Password123@"
            };

            _mockAuthService.Setup(s => s.RegisterStudentAsync(It.IsAny<RegisterRequest>()))
                            .ReturnsAsync((true, "Registration successful", 201));

            // Act
            var result = await _controller.RegisterStudent(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, objectResult.StatusCode);

            // SỬA: Dùng hàm GetProperty để lấy giá trị "message"
            var message = GetProperty<string>(objectResult.Value, "message");
            Assert.Equal("Registration successful", message);
        }

        [Fact(DisplayName = "Đăng ký thất bại do email đã tồn tại trả về mã 400")]
        public async Task RegisterStudent_Returns400_WhenEmailExists()
        {
            // Arrange
            var request = new RegisterRequest { Email = "existed@fpt.edu.vn" };

            _mockAuthService.Setup(s => s.RegisterStudentAsync(It.IsAny<RegisterRequest>()))
                            .ReturnsAsync((false, "Email already exists", 400));

            // Act
            var result = await _controller.RegisterStudent(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);

            // SỬA: Dùng hàm GetProperty
            var message = GetProperty<string>(objectResult.Value, "message");
            Assert.Equal("Email already exists", message);
        }

        [Fact(DisplayName = "Đăng nhập thành công trả về Token và thông tin người dùng")]
        public async Task Login_ReturnsTokenAndInfo_WhenCredentialsValid()
        {
            // Arrange
            var loginReq = new LoginRequest { Email = "test@test.com", Password = "123" };

            // Tuple 10 phần tử khớp với Interface của bạn
            var successTuple = (
                true,                       // Success
                "Login success",            // Message
                200,                        // StatusCode
                "eyJhbGciOi...",            // accessToken
                "refresh-token-123",        // refreshToken
                "USER-ID-101",              // userId (String)
                true,                       // hasActiveContract
                false,                      // hasTerminatedContract
                "Dom A",                    // BuildingName
                "BUILD-01"                  // BuildingID (String)
            );

            _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
                            .ReturnsAsync(successTuple);

            // Act
            var result = await _controller.Login(loginReq);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);

            // SỬA: Dùng Reflection để lấy từng trường dữ liệu ra kiểm tra
            // Lưu ý: Tên property ("accessToken", "buildingName") phải khớp CHÍNH XÁC chữ hoa/thường 
            // trong lệnh: new { accessToken = ..., buildingName = ... } tại Controller

            Assert.Equal("eyJhbGciOi...", GetProperty<string>(objectResult.Value, "accessToken"));
            Assert.Equal("Dom A", GetProperty<string>(objectResult.Value, "buildingName"));
            Assert.Equal("USER-ID-101", GetProperty<string>(objectResult.Value, "userId"));
        }

        [Fact(DisplayName = "Đăng nhập thất bại do sai thông tin trả về mã 401")]
        public async Task Login_Returns401_WhenPasswordWrong()
        {
            // Arrange
            var loginReq = new LoginRequest { Email = "test@test.com", Password = "wrong" };

            var failTuple = (
                false,
                "Invalid credentials",
                401,
                (string)null,
                (string)null,
                (string)null,
                false,
                false,
                (string)null,
                (string)null
            );

            _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
                            .ReturnsAsync(failTuple);

            // Act
            var result = await _controller.Login(loginReq);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(401, objectResult.StatusCode);

            var message = GetProperty<string>(objectResult.Value, "message");
            Assert.Equal("Invalid credentials", message);
        }

        [Fact(DisplayName = "Xác thực email thành công trả về mã 200")]
        public async Task VerifyEmail_Returns200_WhenSuccess()
        {
            // Arrange
            var verifyReq = new VerifyEmailRequest { Email = "a@a.com", OTP= "123456" };

            _mockAuthService.Setup(s => s.VerifyEmailAsync(It.IsAny<VerifyEmailRequest>()))
                            .ReturnsAsync((true, "Email verified", 200));

            // Act
            var result = await _controller.VerifyEmail(verifyReq);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact(DisplayName = "Gửi lại OTP xác thực email thành công trả về mã 200")]
        public async Task ResendOTPVerifyEmail_Returns200_WhenSuccess()
        {
            // Arrange
            string email = "test@test.com";

            _mockAuthService.Setup(s => s.ResendVerificationOtpAsync(email))
                            .ReturnsAsync((true, "OTP Resent", 200));

            // Act
            var result = await _controller.ResendOTPVerifyEmail(email);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
        }

        // --- HELPER METHOD  ---
        
        private T GetProperty<T>(object obj, string propertyName)
        {
            if (obj == null) return default;

            var property = obj.GetType().GetProperty(propertyName);
            if (property == null) return default;

            return (T)property.GetValue(obj);
        }
    }
}