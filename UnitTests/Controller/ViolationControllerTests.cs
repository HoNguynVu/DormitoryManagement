using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using API.Controllers;
using API.Services.Interfaces;
using BusinessObject.DTOs.ViolationDTOs;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace UnitTests.Controller
{
    public class ViolationControllerTests
    {
        private readonly Mock<IViolationService> _mockViolationService;
        private readonly ViolationController _controller;

        public ViolationControllerTests()
        {
            _mockViolationService = new Mock<IViolationService>();
            _controller = new ViolationController(_mockViolationService.Object);
        }

        [Fact(DisplayName = "Tạo vi phạm thành công trả về mã 200 và dữ liệu")]
        public async Task CreateViolation_Returns200_WhenSuccess()
        {
            // Arrange
            var request = new CreateViolationRequest();
            var responseData = new ViolationResponse { ViolationId = "V1", Description = "Test Violation" };

            // Tuple 4 elements: (bool, string, int, ViolationResponse)
            _mockViolationService.Setup(s => s.CreateViolationAsync(It.IsAny<CreateViolationRequest>()))
                                 .ReturnsAsync((true, "Create success", 200, responseData));

            // Act
            var result = await _controller.CreateViolation(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);

            // Check anonymous object properties
            Assert.Equal("Create success", GetProperty<string>(objectResult.Value, "message"));

            var data = GetProperty<ViolationResponse>(objectResult.Value, "data");
            Assert.NotNull(data);
            Assert.Equal("V1", data.ViolationId);
        }

        [Fact(DisplayName = "Cập nhật hướng xử lý thành công trả về mã 200")]
        public async Task UpdateResolution_Returns200_WhenSuccess()
        {
            // Arrange
            var request = new UpdateViolationRequest();
            _mockViolationService.Setup(s => s.UpdateViolationAsync(It.IsAny<UpdateViolationRequest>()))
                                 .ReturnsAsync((true, "Update success", 200));

            // Act
            var result = await _controller.UpdateResolution(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Equal("Update success", GetProperty<string>(objectResult.Value, "message"));
        }

        [Fact(DisplayName = "Lấy danh sách vi phạm theo StudentId thành công trả về mã 200")]
        public async Task GetViolationsByStudentId_ReturnsList()
        {
            // Arrange
            string studentId = "SE123456";
            var mockList = new List<ViolationResponse>
            {
                new ViolationResponse { ViolationId = "V1", Description = "Noise" }
            };

            _mockViolationService.Setup(s => s.GetViolationsByStudentIdAsync(studentId))
                                 .ReturnsAsync((true, "Found", 200, mockList));

            // Act
            var result = await _controller.GetViolationsByStudentId(studentId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);

            var data = GetProperty<IEnumerable<ViolationResponse>>(objectResult.Value, "data");
            Assert.NotNull(data);
            Assert.Single(data);
        }

        [Fact(DisplayName = "Lấy tất cả vi phạm thành công trả về mã 200")]
        public async Task GetAllViolations_ReturnsList()
        {
            // Arrange
            var mockList = new List<ViolationResponse>();

            _mockViolationService.Setup(s => s.GetAllViolationsAsync())
                                 .ReturnsAsync((true, "All violations", 200, mockList));

            // Act
            var result = await _controller.GetAllViolations();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact(DisplayName = "Lấy danh sách vi phạm chưa xử lý (Pending) thành công trả về mã 200")]
        public async Task GetPendingViolations_ReturnsList()
        {
            // Arrange
            var mockList = new List<ViolationResponse>();

            _mockViolationService.Setup(s => s.GetPendingViolationsAsync())
                                 .ReturnsAsync((true, "Pending list", 200, mockList));

            // Act
            var result = await _controller.GetPendingViolations();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact(DisplayName = "Lấy thống kê vi phạm theo quản lý (Dashboard) thành công trả về mã 200")]
        public async Task GetViolationDashboard_ReturnsStats()
        {
            // Arrange
            string managerId = "MN001";

            var mockStats = new List<ViolationStats>
            {
                new ViolationStats { /* properties */ }
            };

            _mockViolationService.Setup(s => s.GetViolationStatsByManagerAsync(managerId))
                                 .ReturnsAsync((true, "Stats retrieved", 200, mockStats));

            // Act
            var result = await _controller.GetViolationDashboard(managerId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);

            var data = GetProperty<IEnumerable<ViolationStats>>(objectResult.Value, "data");
            Assert.NotNull(data);
        }

        [Fact(DisplayName = "Lấy tất cả vi phạm theo quản lý thành công trả về mã 200")]
        public async Task GetAllViolationsByManager_ReturnsList()
        {
            // Arrange
            string managerId = "MN001";
            var mockList = new List<ViolationResponse>();

            _mockViolationService.Setup(s => s.GetAllViolationsByManagerAsync(managerId))
                                 .ReturnsAsync((true, "Manager violations", 200, mockList));

            // Act
            var result = await _controller.GetAllViolationsByManager(managerId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact(DisplayName = "Lấy vi phạm theo tài khoản sinh viên thành công trả về mã 200")]
        public async Task GetViolationsByStudentAccountId_ReturnsList()
        {
            // Arrange
            string accountId = "ACC_ST_001";
            var mockList = new List<ViolationResponse>();

            _mockViolationService.Setup(s => s.GetViolationsByStudentAccountIdAsync(accountId))
                                 .ReturnsAsync((true, "Student violations", 200, mockList));

            // Act
            var result = await _controller.GetViolationsByStudentAccountId(accountId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
        }

        // --- Helper Method ---
        private T GetProperty<T>(object obj, string propertyName)
        {
            if (obj == null) return default;
            var property = obj.GetType().GetProperty(propertyName);
            if (property == null) return default;
            return (T)property.GetValue(obj);
        }
    }
}