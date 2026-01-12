using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using API.Controllers;
using API.Services.Interfaces;
using BusinessObject.DTOs.RegisDTOs;
using System.Reflection;

namespace UnitTests.Controller
{
    public class RegistrationControllerTests
    {
        private readonly Mock<IRegistrationService> _mockRegistrationService;
        private readonly RegistrationController _controller;

        public RegistrationControllerTests()
        {
            _mockRegistrationService = new Mock<IRegistrationService>();
            _controller = new RegistrationController(_mockRegistrationService.Object);
        }

        [Fact(DisplayName = "Tạo đơn đăng ký thành công trả về mã 200 và ID dạng chuỗi")]
        public async Task CreateRegistrationForm_ReturnsSuccess_WhenValid()
        {
            // Arrange
            var request = new RegistrationFormRequest();

            // SỬA: Tham số thứ 4 (registrationId) phải là STRING ("REG-001"), không phải int
            var successTuple = (true, "Form created successfully", 200, "REG-001");

            _mockRegistrationService.Setup(s => s.CreateRegistrationForm(It.IsAny<RegistrationFormRequest>()))
                                    .ReturnsAsync(successTuple);

            // Act
            var result = await _controller.CreateRegistrationForm(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);

            Assert.Equal("Form created successfully", GetProperty<string>(objectResult.Value, "Message"));

            // SỬA: Assert kiểu string thay vì int
            Assert.Equal("REG-001", GetProperty<string>(objectResult.Value, "registrationId"));
        }

        [Fact(DisplayName = "Tạo đơn đăng ký thất bại trả về mã lỗi và thông báo lỗi")]
        public async Task CreateRegistrationForm_ReturnsError_WhenServiceFails()
        {
            // Arrange
            var request = new RegistrationFormRequest();

            // SỬA: Tham số thứ 4 là null (ép kiểu string) hoặc chuỗi rỗng
            var failTuple = (false, "Invalid input data", 400, (string)null);

            _mockRegistrationService.Setup(s => s.CreateRegistrationForm(It.IsAny<RegistrationFormRequest>()))
                                    .ReturnsAsync(failTuple);

            // Act
            var result = await _controller.CreateRegistrationForm(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);

            Assert.Equal("Invalid input data", GetProperty<string>(objectResult.Value, "Error"));
        }

        private T GetProperty<T>(object obj, string propertyName)
        {
            if (obj == null) return default;
            var property = obj.GetType().GetProperty(propertyName);
            if (property == null) return default;
            return (T)property.GetValue(obj);
        }
    }
}