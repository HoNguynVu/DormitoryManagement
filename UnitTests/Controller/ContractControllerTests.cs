using API.Controllers;
using API.Services.Interfaces;
using BusinessObject.DTOs.ContractDTOs;
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
    public class ContractControllerTests
    {
        private readonly Mock<IContractService> _mockContractService;
        private readonly Mock<IPaymentService> _mockPaymentService;
        private readonly ContractController _controller;

        public ContractControllerTests()
        {
            _mockContractService = new Mock<IContractService>();
            _mockPaymentService = new Mock<IPaymentService>();
            _controller = new ContractController(_mockContractService.Object, _mockPaymentService.Object);
        }

        [Fact(DisplayName = "GetContracts returns 200 and data when success")]
        public async Task GetContracts_Returns200_WhenSuccess()
        {
            // Arrange
            var mockList = new List<SummaryContractDto>();
            _mockContractService.Setup(s => s.GetContractFiltered(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>()))
                                .ReturnsAsync((true, "Success", 200, mockList));

            // Act
            var result = await _controller.GetContracts(null, null, null, null, null);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Equal("Success", GetProperty<string>(objectResult.Value, "message"));
            Assert.NotNull(GetProperty<IEnumerable<SummaryContractDto>>(objectResult.Value, "data"));
        }

        [Fact(DisplayName = "GetContracts returns error status when service fails")]
        public async Task GetContracts_ReturnsError_WhenFail()
        {
            // Arrange
            _mockContractService.Setup(s => s.GetContractFiltered(null, null, null, null, null))
                                .ReturnsAsync((false, "Internal Error", 500, null));

            // Act
            var result = await _controller.GetContracts(null, null, null, null, null);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact(DisplayName = "GetContractById returns 200 and detail")]
        public async Task GetContractById_Returns200_WhenFound()
        {
            // Arrange
            string id = "CT001";
            var mockDetail = new DetailContractDto();
            _mockContractService.Setup(s => s.GetDetailContract(id))
                                .ReturnsAsync((true, "Found", 200, mockDetail));

            // Act
            var result = await _controller.GetContractById(id);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.NotNull(GetProperty<DetailContractDto>(objectResult.Value, "data"));
        }

        [Fact(DisplayName = "GetContractOverview returns 200 and stats")]
        public async Task GetContractOverview_Returns200_WhenSuccess()
        {
            // Arrange
            var mockStat = new Dictionary<string, int> { { "Active", 10 } };
            _mockContractService.Setup(s => s.GetOverviewContract(null))
                                .ReturnsAsync((true, "Stats", 200, mockStat));

            // Act
            var result = await _controller.GetContractOverview(null);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            var data = GetProperty<Dictionary<string, int>>(objectResult.Value, "data");
            Assert.Equal(10, data["Active"]);
        }

        [Fact(DisplayName = "RequestRenewal returns 200 and receiptId when success")]
        public async Task RequestRenewal_Returns200_WhenSuccess()
        {
            // Arrange
            var request = new RenewalRequestDto { StudentId = "ST01", MonthsToExtend = 3 };
            _mockContractService.Setup(s => s.RequestRenewalAsync("ST01", 3))
                                .ReturnsAsync((true, "Renewal request created", 201, "RCPT001"));

            // Act
            var result = await _controller.RequestRenewal(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, objectResult.StatusCode);
            Assert.Equal("RCPT001", GetProperty<string>(objectResult.Value, "receiptId"));
        }

        [Fact(DisplayName = "RejectRenewal returns 200 when success")]
        public async Task RejectRenewal_Returns200_WhenSuccess()
        {
            // Arrange
            var dto = new RejectRenewalDto();
            _mockContractService.Setup(s => s.RejectRenewalAsync(dto))
                                .ReturnsAsync((true, "Rejected", 200));

            // Act
            var result = await _controller.RejectRenewal(dto);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact(DisplayName = "TerminateContract returns 200 when success")]
        public async Task TerminateContract_Returns200_WhenSuccess()
        {
            // Arrange
            string studentId = "ST01";
            _mockContractService.Setup(s => s.TerminateContractNowAsync(studentId))
                                .ReturnsAsync((true, "Terminated", 200));

            // Act
            var result = await _controller.TerminateContract(studentId);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact(DisplayName = "ChangeRoom returns BadRequest if request is null")]
        public async Task ChangeRoom_ReturnsBadRequest_WhenNull()
        {
            // Act
            var result = await _controller.ChangeRoom(null);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact(DisplayName = "ChangeRoom returns Success with PaymentUrl when Charge type")]
        public async Task ChangeRoom_ReturnsPaymentUrl_WhenChargeType()
        {
            // Arrange
            var request = new ChangeRoomRequestDto();
            // Contract Service returns a receipt and type "Charge"
            _mockContractService.Setup(s => s.ChangeRoomAsync(request))
                                .ReturnsAsync((true, "Room changed", 200, "RCPT_NEW", "Charge"));

            // Mock Payment Service to return success url
            var mockPaymentReturn = new PaymentLinkDTO{ IsSuccess = true, PaymentUrl = "http://zalopay.url", PaymentId = "TRANS01" };
            _mockPaymentService.Setup(s => s.CreateZaloPayLinkForRoomChange("RCPT_NEW"))
                               .ReturnsAsync((200, mockPaymentReturn));

            // Act
            var result = await _controller.ChangeRoom(request);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Equal("Charge", GetProperty<string>(objectResult.Value, "type"));
            Assert.Equal("http://zalopay.url", GetProperty<string>(objectResult.Value, "paymentUrl"));
        }

        [Fact(DisplayName = "ChangeRoom returns PaymentError when PaymentService fails")]
        public async Task ChangeRoom_ReturnsError_WhenPaymentFails()
        {
            // Arrange
            var request = new ChangeRoomRequestDto();
            _mockContractService.Setup(s => s.ChangeRoomAsync(request))
                                .ReturnsAsync((true, "Room changed", 200, "RCPT_NEW", "Charge"));

            // Mock Payment Service to fail
            var mockPaymentReturn = new PaymentLinkDTO { IsSuccess = false, Message = "Zalo error" };
            _mockPaymentService.Setup(s => s.CreateZaloPayLinkForRoomChange("RCPT_NEW"))
                               .ReturnsAsync((500, mockPaymentReturn));

            // Act
            var result = await _controller.ChangeRoom(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Zalo error", GetProperty<string>(objectResult.Value, "paymentError"));
        }

        [Fact(DisplayName = "ChangeRoom returns Success without PaymentUrl when No Charge")]
        public async Task ChangeRoom_ReturnsSuccess_WhenNoCharge()
        {
            // Arrange
            var request = new ChangeRoomRequestDto();
            // Type is "None" or "Refund" -> No payment link creation
            _mockContractService.Setup(s => s.ChangeRoomAsync(request))
                                .ReturnsAsync((true, "Room changed free", 200, "RCPT_FREE", "None"));

            // Act
            var result = await _controller.ChangeRoom(request);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Null(GetProperty<string>(objectResult.Value, "paymentUrl"));
            // Verify Payment Service was NEVER called
            _mockPaymentService.Verify(s => s.CreateZaloPayLinkForRoomChange(It.IsAny<string>()), Times.Never);
        }

        [Fact(DisplayName = "RemindBulkExpiringContracts returns Ok when success")]
        public async Task RemindBulkExpiringContracts_ReturnsOk()
        {
            // Arrange
            _mockContractService.Setup(s => s.RemindBulkExpiringAsync())
                                .ReturnsAsync((true, "Reminders sent"));

            // Act
            var result = await _controller.RemindExpiringContracts();

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, objectResult.StatusCode);
        }

        [Fact(DisplayName = "RemindSingleStudent returns BadRequest when fail")]
        public async Task RemindSingleStudent_ReturnsBadRequest_WhenFail()
        {
            // Arrange
            string id = "ST01";
            _mockContractService.Setup(s => s.RemindSingleStudentAsync(id))
                                .ReturnsAsync((false, "Student not found"));

            // Act
            var result = await _controller.RemindSingleStudent(id);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
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
