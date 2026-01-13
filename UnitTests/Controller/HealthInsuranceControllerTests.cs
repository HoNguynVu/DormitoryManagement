using API.Controllers;
using API.Services.Interfaces;
using BusinessObject.DTOs.HealthInsuranceDTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Controller
{
    public class HealthInsuranceControllerTests
    {
        private readonly Mock<IHealthInsuranceService> _mockService;
        private readonly HealthInsuranceController _controller;

        public HealthInsuranceControllerTests()
        {
            _mockService = new Mock<IHealthInsuranceService>();
            _controller = new HealthInsuranceController(_mockService.Object);
        }

        [Fact(DisplayName = "Lấy danh sách BHYT (Filter) thành công trả về 200")]
        public async Task GetHealthInsurances_Returns200_WhenSuccess()
        {
            // Arrange
            var mockList = new List<SummaryHealthDto> { new SummaryHealthDto() };
            _mockService.Setup(s => s.GetHealthInsuranceFiltered(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<string?>()))
                        .ReturnsAsync((true, "Success", 200, mockList));

            // Act
            var result = await _controller.GetHealthInsurances(null, null, null, null);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.NotNull(GetProperty<IEnumerable<SummaryHealthDto>>(objectResult.Value, "data"));
        }

        [Fact(DisplayName = "Lấy chi tiết BHYT theo ID thành công trả về 200")]
        public async Task GetDetailHealthInsurance_Returns200_WhenFound()
        {
            // Arrange
            string id = "HI_001";
            var mockDto = new HealthDetailDto();
            _mockService.Setup(s => s.GetDetailHealth(id))
                        .ReturnsAsync((true, "Found", 200, mockDto));

            // Act
            var result = await _controller.GetDetailHealthInsurance(id);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.NotNull(GetProperty<HealthDetailDto>(objectResult.Value, "data"));
        }

        [Fact(DisplayName = "Lấy chi tiết BHYT thất bại trả về 404")]
        public async Task GetDetailHealthInsurance_Returns404_WhenNotFound()
        {
            // Arrange
            string id = "HI_999";
            _mockService.Setup(s => s.GetDetailHealth(id))
                        .ReturnsAsync((false, "Not Found", 404, null));

            // Act
            var result = await _controller.GetDetailHealthInsurance(id);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact(DisplayName = "Lấy giá BHYT theo năm thành công trả về 200")]
        public async Task GetPriceHealthInsurance_Returns200_WhenSuccess()
        {
            // Arrange
            int year = 2024;
            var mockDto = new HealthPriceDto();
            _mockService.Setup(s => s.GetHealthPriceByYear(year))
                        .ReturnsAsync((true, "Success", 200, mockDto));

            // Act
            var result = await _controller.GetPriceHealthInsurance(year);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.NotNull(GetProperty<HealthPriceDto>(objectResult.Value, "data"));
        }

        [Fact(DisplayName = "Lấy BHYT theo sinh viên thành công trả về 200")]
        public async Task GetStudentInsurance_Returns200_WhenSuccess()
        {
            // Arrange
            string studentId = "STU_001";
            var mockDto = new SummaryHealthDto();
            _mockService.Setup(s => s.GetInsuranceByStudentIdAsync(studentId))
                        .ReturnsAsync((true, "Found", 200, mockDto));

            // Act
            var result = await _controller.GetStudentInsurance(studentId);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.NotNull(GetProperty<SummaryHealthDto>(objectResult.Value, "data"));
        }

        [Fact(DisplayName = "Lấy danh sách bệnh viện thành công trả về 200")]
        public async Task GetAllHospital_Returns200_WhenSuccess()
        {
            // Arrange
            var mockList = new List<SummaryHospitalDto>();
            _mockService.Setup(s => s.GetAllHospitalAsync())
                        .ReturnsAsync((true, "Success", 200, mockList));

            // Act
            var result = await _controller.GetAllHospital();

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.NotNull(GetProperty<IEnumerable<SummaryHospitalDto>>(objectResult.Value, "data"));
        }

        [Fact(DisplayName = "Đăng ký BHYT (Register) thành công trả về 201 và InsuranceId")]
        public async Task RegisterInsurance_Returns201_WhenSuccess()
        {
            // Arrange
            var request = new HealthInsuranceRequestDto { StudentId = "S1", HospitalId = "H1", CardNumber = "123" };
            string expectedId = "INS_NEW_001";

            _mockService.Setup(s => s.RegisterHealthInsuranceAsync(request.StudentId, request.HospitalId, request.CardNumber))
                        .ReturnsAsync((true, "Created", 201, expectedId));

            // Act
            var result = await _controller.RegisterInsurance(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, objectResult.StatusCode);
            Assert.Equal(expectedId, GetProperty<string>(objectResult.Value, "insuranceId"));
        }

        [Fact(DisplayName = "Đăng ký BHYT với request null trả về BadRequest")]
        public async Task RegisterInsurance_Returns400_WhenRequestNull()
        {
            // Act
            var result = await _controller.RegisterInsurance(null);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact(DisplayName = "Xác nhận thanh toán thành công trả về 200")]
        public async Task ConfirmPayment_Returns200_WhenSuccess()
        {
            // Arrange
            string id = "INS_001";
            _mockService.Setup(s => s.ConfirmInsurancePaymentAsync(id))
                        .ReturnsAsync((true, "Confirmed", 200));

            // Act
            var result = await _controller.ConfirmPayment(id);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Equal("Confirmed", GetProperty<string>(objectResult.Value, "message"));
        }

        [Fact(DisplayName = "Tạo giá BHYT mới thành công trả về 201 và PriceId")]
        public async Task CreateHealthPrice_Returns201_WhenSuccess()
        {
            // Arrange
            var dto = new CreateHealthPriceDTO();
            string expectedPriceId = "PRICE_2025";

            _mockService.Setup(s => s.CreateHealthInsurancePriceAsync(dto))
                        .ReturnsAsync((true, "Created", 201, expectedPriceId));

            // Act
            var result = await _controller.CreateHealthPrice(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, objectResult.StatusCode);
            Assert.Equal(expectedPriceId, GetProperty<string>(objectResult.Value, "priceId"));
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
