using API.Controllers;
using API.Services.Interfaces;
using BusinessObject.DTOs.PaymentDTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Controller
{
    public class PaymentControllerTests
    {
        private readonly Mock<IPaymentService> _mockService;
        private readonly PaymentController _controller;

        public PaymentControllerTests()
        {
            _mockService = new Mock<IPaymentService>();
            _controller = new PaymentController(_mockService.Object);
        }

        [Fact(DisplayName = "Tạo link thanh toán đăng ký thành công trả về 200")]
        public async Task CreateZaloPayLinkForRegistration_Returns200_WhenSuccess()
        {
            // Arrange
            string regId = "REG001";
            var mockDto = new PaymentLinkDTO();
            _mockService.Setup(s => s.CreateZaloPayLinkForRegistration(regId))
                        .ReturnsAsync((200, mockDto));

            // Act
            var result = await _controller.CreateZaloPayLinkForRegistration(regId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Same(mockDto, objectResult.Value);
        }

        [Fact(DisplayName = "Tạo link thanh toán gia hạn hợp đồng thành công trả về 200")]
        public async Task CreateZaloPayLinkForContract_Returns200_WhenSuccess()
        {
            // Arrange
            string receiptId = "RCPT001";
            var mockDto = new PaymentLinkDTO();
            _mockService.Setup(s => s.CreateZaloPayLinkForRenewal(receiptId))
                        .ReturnsAsync((200, mockDto));

            // Act
            var result = await _controller.CreateZaloPayLinkForContract(receiptId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Same(mockDto, objectResult.Value);
        }

        [Fact(DisplayName = "Tạo link thanh toán điện nước thành công trả về 200")]
        public async Task CreateZaloPayLinkForUtility_Returns200_WhenSuccess()
        {
            // Arrange
            string utilityId = "UTIL001";
            string accountId = "ACC001";
            var mockDto = new PaymentLinkDTO();
            _mockService.Setup(s => s.CreateZaloPayLinkForUtility(utilityId, accountId))
                        .ReturnsAsync((200, mockDto));

            // Act
            var result = await _controller.CreateZaloPayLinkForUtility(utilityId, accountId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Same(mockDto, objectResult.Value);
        }

        [Fact(DisplayName = "Tạo link thanh toán BHYT thành công trả về 200")]
        public async Task CreateZaloPayLinkForHealthInsurance_Returns200_WhenSuccess()
        {
            // Arrange
            string insuranceId = "INS001";
            var mockDto = new PaymentLinkDTO();
            _mockService.Setup(s => s.CreateZaloPayLinkForHealthInsurance(insuranceId))
                        .ReturnsAsync((200, mockDto));

            // Act
            var result = await _controller.CreateZaloPayLinkForHealthInsurance(insuranceId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Same(mockDto, objectResult.Value);
        }

        [Fact(DisplayName = "Tạo link thanh toán đổi phòng thành công trả về 200")]
        public async Task CreateZaloPayLinkForRoomChange_Returns200_WhenSuccess()
        {
            // Arrange
            string receiptId = "RCPT_ROOM_001";
            var mockDto = new PaymentLinkDTO();
            _mockService.Setup(s => s.CreateZaloPayLinkForRoomChange(receiptId))
                        .ReturnsAsync((200, mockDto));

            // Act
            var result = await _controller.CreateZaloPayLinkForRoomChange(receiptId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Same(mockDto, objectResult.Value);
        }

        [Fact(DisplayName = "Tạo link thanh toán bảo trì thành công trả về 200")]
        public async Task CreateZaloPayLinkForMaintenance_Returns200_WhenSuccess()
        {
            // Arrange
            string receiptId = "RCPT_MAIN_001";
            var mockDto = new PaymentLinkDTO();
            _mockService.Setup(s => s.CreateZaloPayLinkForMaintenance(receiptId))
                        .ReturnsAsync((200, mockDto));

            // Act
            var result = await _controller.CreateZaloPayLinkForMaintenance(receiptId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Same(mockDto, objectResult.Value);
        }

        [Fact(DisplayName = "Xử lý Callback ZaloPay trả về kết quả đúng")]
        public async Task ZaloPayCallback_ReturnsOk_WithCorrectData()
        {
            // Arrange
            var cbData = new ZaloPayCallbackDTO { Data = "sample", Mac = "sample", Type = 1 };
            _mockService.Setup(s => s.ProcessZaloPayCallback(cbData))
                        .ReturnsAsync((1, "Success"));

            // Act
            var result = await _controller.ZaloPayCallback(cbData);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);

            Assert.Equal(1, GetProperty<int>(objectResult.Value, "return_code"));
            Assert.Equal("Success", GetProperty<string>(objectResult.Value, "return_message"));
        }

        [Fact(DisplayName = "Lấy kết quả thanh toán theo AppTransId thành công trả về 200")]
        public async Task GetPaymentResultByAppTransId_Returns200_WhenSuccess()
        {
            // Arrange
            string appTransId = "TRANS_001";
            var mockDto = new PaymentResultDto();
            _mockService.Setup(s => s.GetPaymentResultByAppTransId(appTransId))
                        .ReturnsAsync((200, mockDto));

            // Act
            var result = await _controller.GetPaymentResultByAppTransId(appTransId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Same(mockDto, objectResult.Value);
        }

        [Fact(DisplayName = "Lấy kết quả thanh toán trả về lỗi khi Service báo lỗi")]
        public async Task GetPaymentResultByAppTransId_ReturnsError_WhenServiceFails()
        {
            // Arrange
            string appTransId = "TRANS_FAIL";
            _mockService.Setup(s => s.GetPaymentResultByAppTransId(appTransId))
                        .ReturnsAsync((404, null));

            // Act
            var result = await _controller.GetPaymentResultByAppTransId(appTransId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, objectResult.StatusCode);
            Assert.Null(objectResult.Value);
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
