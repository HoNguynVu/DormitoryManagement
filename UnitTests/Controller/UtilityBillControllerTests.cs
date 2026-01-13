using API.Controllers;
using API.Services.Interfaces;
using BusinessObject.DTOs.UtilityBillDTOs;
using BusinessObject.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Controller
{
    public class UtilityBillControllerTests
    {
        private readonly Mock<IUtilityBillService> _mockService;
        private readonly UtilityBillController _controller;

        public UtilityBillControllerTests()
        {
            _mockService = new Mock<IUtilityBillService>();
            _controller = new UtilityBillController(_mockService.Object);
        }

        [Fact(DisplayName = "Lấy hóa đơn theo sinh viên thành công trả về 200")]
        public async Task GetUtilityBillsByStudent_Returns200_WhenSuccess()
        {
            // Arrange
            string accountId = "ACC001";
            var mockList = new List<UtilityBillDetailForStudent>
            {
                new UtilityBillDetailForStudent()
            };

            _mockService.Setup(s => s.GetUtilityBillsByStudent(accountId))
                        .ReturnsAsync((true, "Get success", 200, mockList));

            // Act
            var result = await _controller.GetUtilityBillsByStudent(accountId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Equal("Get success", GetProperty<string>(objectResult.Value, "message"));
            Assert.NotNull(GetProperty<IEnumerable<UtilityBillDetailForStudent>>(objectResult.Value, "data"));
        }

        [Fact(DisplayName = "Lấy hóa đơn theo sinh viên thất bại trả về 404")]
        public async Task GetUtilityBillsByStudent_Returns404_WhenNotFound()
        {
            // Arrange
            string accountId = "ACC001";

            _mockService.Setup(s => s.GetUtilityBillsByStudent(accountId))
                        .ReturnsAsync((false, "Not found", 404, null));

            // Act
            var result = await _controller.GetUtilityBillsByStudent(accountId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, objectResult.StatusCode);
            Assert.Equal("Not found", GetProperty<string>(objectResult.Value, "message"));
        }

        [Fact(DisplayName = "Tạo hóa đơn thành công trả về 200")]
        public async Task CreateUtilityBill_Returns200_WhenSuccess()
        {
            // Arrange
            var dto = new CreateBillDTO();

            _mockService.Setup(s => s.CreateUtilityBill(It.IsAny<CreateBillDTO>()))
                        .ReturnsAsync((true, "Created successfully", 200));

            // Act
            var result = await _controller.CreateUtilityBill(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Equal("Created successfully", GetProperty<string>(objectResult.Value, "message"));
        }

        [Fact(DisplayName = "Tạo hóa đơn thất bại trả về 400")]
        public async Task CreateUtilityBill_Returns400_WhenFail()
        {
            // Arrange
            var dto = new CreateBillDTO();

            _mockService.Setup(s => s.CreateUtilityBill(It.IsAny<CreateBillDTO>()))
                        .ReturnsAsync((false, "Creation failed", 400));

            // Act
            var result = await _controller.CreateUtilityBill(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
            Assert.Equal("Creation failed", GetProperty<string>(objectResult.Value, "message"));
        }

        [Fact(DisplayName = "Quản lý lấy danh sách hóa đơn thành công trả về 200")]
        public async Task GetUtilityBillsByManager_Returns200_WhenSuccess()
        {
            // Arrange
            var request = new ManagerGetBillRequest();
            var mockList = new List<ManagerGetBillDTO> { new ManagerGetBillDTO() };

            _mockService.Setup(s => s.GetBillsForManagerAsync(It.IsAny<ManagerGetBillRequest>()))
                        .ReturnsAsync((true, "Success", 200, mockList));

            // Act
            var result = await _controller.GetUtilityBillsByManager(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.NotNull(GetProperty<IEnumerable<ManagerGetBillDTO>>(objectResult.Value, "data"));
        }

        [Fact(DisplayName = "Lấy thông số hiện hành thành công trả về 200")]
        public async Task GetActiveParameter_Returns200_WhenSuccess()
        {
            // Arrange
            var mockPara = new Parameter
            {
                ParameterID = 1,
                DefaultElectricityPrice = 3500.00m,
                DefaultWaterPrice = 6000.00m,
                IsActive = true,
                EffectiveDate = DateTime.Now
            };

            _mockService.Setup(s => s.GetActiveParameter())
                        .ReturnsAsync((true, "Active param found", 200, mockPara));

            // Act
            var result = await _controller.GetActiveParameter();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);

            var data = GetProperty<Parameter>(objectResult.Value, "data");
            Assert.NotNull(data);
            Assert.Equal(1, data.ParameterID);
            Assert.Equal(3500.00m, data.DefaultElectricityPrice);
        }

        [Fact(DisplayName = "Lấy chỉ số tháng trước thành công trả về 200")]
        public async Task GetLastMonthIndex_Returns200_WhenSuccess()
        {
            // Arrange
            var request = new RequestLastMonthIndexDTO();
            var mockDto = new LastMonthIndexDTO();

            _mockService.Setup(s => s.GetLastMonthIndex(It.IsAny<RequestLastMonthIndexDTO>()))
                        .ReturnsAsync((true, "Index found", 200, mockDto));

            // Act
            var result = await _controller.GetLastMonthIndex(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.NotNull(GetProperty<LastMonthIndexDTO>(objectResult.Value, "data"));
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
