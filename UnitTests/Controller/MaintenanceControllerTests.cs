using API.Controllers;
using API.Services.Interfaces;
using BusinessObject.DTOs.MaintenanceDTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Controller
{
    public class MaintenanceControllerTests
    {
        private readonly Mock<IMaintenanceService> _mockService;
        private readonly MaintenanceController _controller;

        public MaintenanceControllerTests()
        {
            _mockService = new Mock<IMaintenanceService>();
            _controller = new MaintenanceController(_mockService.Object);
        }

        [Fact(DisplayName = "Tạo yêu cầu bảo trì thành công trả về 201")]
        public async Task CreateRequest_Returns201_WhenSuccess()
        {
            // Arrange
            var dto = new CreateMaintenanceDto();
            string expectedId = "REQ001";
            _mockService.Setup(s => s.CreateRequestAsync(dto))
                        .ReturnsAsync((true, "Created successfully", 201, expectedId));

            // Act
            var result = await _controller.CreateRequest(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, objectResult.StatusCode);
            Assert.Equal(expectedId, GetProperty<string>(objectResult.Value, "data"));
        }

        [Fact(DisplayName = "Tạo yêu cầu bảo trì thất bại trả về 400")]
        public async Task CreateRequest_Returns400_WhenFail()
        {
            // Arrange
            var dto = new CreateMaintenanceDto();
            _mockService.Setup(s => s.CreateRequestAsync(dto))
                        .ReturnsAsync((false, "Creation failed", 400, null));

            // Act
            var result = await _controller.CreateRequest(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
            Assert.Equal("Creation failed", GetProperty<string>(objectResult.Value, "message"));
        }

        [Fact(DisplayName = "Tạo yêu cầu bảo trì với DTO null trả về BadRequest")]
        public async Task CreateRequest_ReturnsBadRequest_WhenNull()
        {
            // Act
            var result = await _controller.CreateRequest(null);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact(DisplayName = "Lấy danh sách theo StudentId thành công trả về 200")]
        public async Task GetMaintenances_Returns200_WhenStudentIdProvided()
        {
            // Arrange
            string studentId = "STU001";
            var mockList = new List<SummaryMaintenanceDto> { new SummaryMaintenanceDto() };
            _mockService.Setup(s => s.GetRequestsByStudentIdAsync(studentId))
                        .ReturnsAsync((true, "Success", 200, mockList));

            // Act
            var result = await _controller.GetMaintenances(studentId, null, null, null, null);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.NotNull(GetProperty<IEnumerable<SummaryMaintenanceDto>>(objectResult.Value, "data"));
        }

        [Fact(DisplayName = "Lấy danh sách filter (không có StudentId) thành công trả về 200")]
        public async Task GetMaintenances_Returns200_WhenFiltered()
        {
            // Arrange
            var mockList = new List<SummaryMaintenanceDto> { new SummaryMaintenanceDto() };
            _mockService.Setup(s => s.GetMaintenanceFiltered(null, "Pending", null, null))
                        .ReturnsAsync((true, "Success", 200, mockList));

            // Act
            var result = await _controller.GetMaintenances(null, null, "Pending", null, null);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.NotNull(GetProperty<IEnumerable<SummaryMaintenanceDto>>(objectResult.Value, "data"));
        }

        [Fact(DisplayName = "Lấy chi tiết yêu cầu bảo trì thành công trả về 200")]
        public async Task GetMaintenanceDetail_Returns200_WhenFound()
        {
            // Arrange
            string id = "REQ001";
            var mockDto = new DetailMaintenanceDto();
            _mockService.Setup(s => s.GetMaintenanceDetail(id))
                        .ReturnsAsync((true, "Found", 200, mockDto));

            // Act
            var result = await _controller.GetMaintenanceDetail(id);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.NotNull(GetProperty<DetailMaintenanceDto>(objectResult.Value, "data"));
        }

        [Fact(DisplayName = "Cập nhật trạng thái thành công trả về 200")]
        public async Task UpdateStatus_Returns200_WhenSuccess()
        {
            // Arrange
            string id = "REQ001";
            var dto = new UpdateMaintenanceStatusDto();
            _mockService.Setup(s => s.UpdateStatusAsync(dto))
                        .ReturnsAsync((true, "Updated", 200));

            // Act
            var result = await _controller.UpdateStatus(id, dto);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Equal("Updated", GetProperty<string>(objectResult.Value, "message"));
        }

        [Fact(DisplayName = "Lấy thông tin tổng quan thành công trả về 200")]
        public async Task GetOverviewMaintenance_Returns200_WhenSuccess()
        {
            // Arrange
            var mockData = new Dictionary<string, int>();
            _mockService.Setup(s => s.GetOverviewMaintenance())
                        .ReturnsAsync((true, "Success", 200, mockData));

            // Act
            var result = await _controller.GetOverviewMaintenance();

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.NotNull(GetProperty<Dictionary<string, int>>(objectResult.Value, "data"));
        }

        [Fact(DisplayName = "Lấy ReceiptId theo RequestId thành công trả về 200")]
        public async Task GetReceiptIdByRequestId_Returns200_WhenSuccess()
        {
            // Arrange
            string requestId = "REQ001";
            string receiptId = "RCPT001";
            _mockService.Setup(s => s.GetReceiptPendingMaintenance(requestId))
                        .ReturnsAsync((true, "Found", 200, receiptId));

            // Act
            var result = await _controller.GetReceiptIdByRequestId(requestId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Equal(receiptId, GetProperty<string>(objectResult.Value, "data"));
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
