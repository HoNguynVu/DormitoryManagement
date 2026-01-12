using API.Services.Implements;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.Config;
using BusinessObject.Entities;
using DataAccess.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;
using Xunit;

namespace UnitTests.Services.Helpers
{
    public class PaymentServicePrivateTests
    {
        private readonly Mock<IOptions<ZaloPaySettings>> _mockOptions;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<IPaymentUow> _mockPaymentUow;
        private readonly Mock<IContractUow> _mockContractUow;
        private readonly Mock<IRegistrationService> _mockRegisService;
        private readonly Mock<IContractService> _mockContractService;
        private readonly Mock<IUtilityBillService> _mockUtilityService;
        private readonly Mock<IHealthInsuranceService> _mockHealthService;
        private readonly Mock<IMaintenanceService> _mockMaintenanceService;
        private readonly Mock<ILogger<PaymentService>> _mockLogger;

        private readonly PaymentService _service;
        private readonly ZaloPaySettings _zaloSettings;

        public PaymentServicePrivateTests()
        {
            // 1. Config Settings
            _zaloSettings = new ZaloPaySettings
            {
                AppId = "2554",
                Key1 = "sdngKKJmqEMzvh5YYrYE",
                Key2 = "trMrHtvjo6myautxDUi",
                CreateOrderUrl = "https://sb-openapi.zalopay.vn/v2/create",
                CallbackUrl = "https://epiphyllous-vocationally-emmaline.ngrok-free.dev/api/payment/callback",
                FrontEndUrl = "http://localhost:5173/student/payment-result"
            };
            _mockOptions = new Mock<IOptions<ZaloPaySettings>>();
            _mockOptions.Setup(x => x.Value).Returns(_zaloSettings);

            // 2. Mock UoW
            _mockPaymentUow = new Mock<IPaymentUow>();
            _mockContractUow = new Mock<IContractUow>();

            // Setup các Repository con của PaymentUow
            _mockPaymentUow.Setup(u => u.Payments).Returns(new Mock<IPaymentRepository>().Object);
            _mockPaymentUow.Setup(u => u.Receipts).Returns(new Mock<IReceiptRepository>().Object);
            _mockPaymentUow.Setup(u => u.Contracts).Returns(new Mock<IContractRepository>().Object);

            // Mock Transaction
            _mockPaymentUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockPaymentUow.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            // 3. Mock Services
            _mockRegisService = new Mock<IRegistrationService>();
            _mockContractService = new Mock<IContractService>();
            _mockUtilityService = new Mock<IUtilityBillService>();
            _mockHealthService = new Mock<IHealthInsuranceService>();
            _mockMaintenanceService = new Mock<IMaintenanceService>();
            _mockLogger = new Mock<ILogger<PaymentService>>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();

            // 4. Init Service
            _service = new PaymentService(
                _mockOptions.Object,
                _mockHttpClientFactory.Object,
                _mockPaymentUow.Object,
                _mockContractUow.Object,
                _mockLogger.Object,
                _mockHealthService.Object,
                _mockRegisService.Object,
                _mockContractService.Object,
                _mockMaintenanceService.Object,
                _mockUtilityService.Object
            );
        }

        // Helper để gọi Private Method
        private async Task<T> InvokePrivateMethodAsync<T>(string methodName, params object[] parameters)
        {
            var method = typeof(PaymentService).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) throw new ArgumentException($"Method {methodName} not found.");
            return await (Task<T>)method.Invoke(_service, parameters);
        }

        #region CallZaloPayCreateOrder Tests

        [Fact(DisplayName = "CallZaloPayCreateOrder: Trả về Order URL khi gọi API thành công")]
        public async Task CallZaloPayCreateOrder_ReturnsUrl_WhenSuccess()
        {
            // Arrange
            string expectedUrl = "https://zalopay.vn/pay/v2/123";
            var successResponse = new
            {
                return_code = 1,
                order_url = expectedUrl,
                zp_trans_token = "token123"
            };

            // Setup HttpClient Mock
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(successResponse))
                });

            var client = new HttpClient(mockHandler.Object);
            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            // Act
            // Gọi hàm private
            var result = await InvokePrivateMethodAsync<string>(
                "CallZaloPayCreateOrder",
                "TRANS_001", 100000L, "Thanh toan test", "ITEM_01"
            );

            // Assert
            Assert.Equal(expectedUrl, result);
        }

        [Fact(DisplayName = "CallZaloPayCreateOrder: Ném Exception khi ZaloPay trả về lỗi")]
        public async Task CallZaloPayCreateOrder_ThrowsException_WhenZaloPayFails()
        {
            // Arrange
            var errorResponse = new
            {
                return_code = 0, // Mã lỗi
                return_message = "Failed",
                sub_return_code = -1,
                sub_return_message = "System error"
            };

            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(errorResponse))
                });

            var client = new HttpClient(mockHandler.Object);
            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                InvokePrivateMethodAsync<string>("CallZaloPayCreateOrder", "TRANS_FAIL", 50000L, "Test Fail", "ITEM_FAIL")
            );

            // Kiểm tra thông báo lỗi bên trong Exception
            Assert.Contains("ZaloPay Error", ex.Message);
        }

        #endregion

        #region HandleRegisSuccessPayment (Test luồng ExecutePaymentTransaction)

        [Fact(DisplayName = "HandleRegisSuccessPayment: Thành công và Commit transaction")]
        public async Task HandleRegisSuccessPayment_Success_WhenValid()
        {
            // Arrange
            string appTransId = "TRANS_REGIS_01";
            string zpTransId = "ZP_123456";
            string receiptId = "REC_01";
            string regisId = "REG_01";

            var payment = new Payment { TransactionID = appTransId, ReceiptID = receiptId, Status = "Pending" };
            var receipt = new Receipt { ReceiptID = receiptId, RelatedObjectID = regisId, Status = "Pending", Amount = 500000 };

            // Mock Data Access
            var mockPaymentRepo = Mock.Get(_mockPaymentUow.Object.Payments);
            mockPaymentRepo.Setup(x => x.GetByIdAsync(appTransId)).ReturnsAsync(payment);

            var mockReceiptRepo = Mock.Get(_mockPaymentUow.Object.Receipts);
            mockReceiptRepo.Setup(x => x.GetByIdAsync(receiptId)).ReturnsAsync(receipt);

            // Mock Registration Service Logic
            _mockRegisService.Setup(x => x.ConfirmPaymentForRegistration(regisId))
                .ReturnsAsync((true, "Confirmed", 200));

            // Act
            var result = await InvokePrivateMethodAsync<(bool Success, string Message, int StatusCode)>(
                "HandleRegisSuccessPayment", appTransId, zpTransId
            );

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);

            // Verify DB Updates
            Assert.Equal("Success", payment.Status); 
            Assert.Equal(zpTransId, payment.TransactionID);

            // Verify Commit được gọi
            _mockPaymentUow.Verify(x => x.CommitAsync(), Times.Once);
            _mockRegisService.Verify(x => x.ConfirmPaymentForRegistration(regisId), Times.Once);
        }

        [Fact(DisplayName = "HandleRegisSuccessPayment: Thất bại (404) khi không tìm thấy Payment")]
        public async Task HandleRegisSuccessPayment_Returns404_WhenPaymentNotFound()
        {
            // Arrange
            var mockPaymentRepo = Mock.Get(_mockPaymentUow.Object.Payments);
            mockPaymentRepo.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((Payment)null);

            // Act
            var result = await InvokePrivateMethodAsync<(bool Success, string Message, int StatusCode)>(
                "HandleRegisSuccessPayment", "MISSING_TRANS", "ZP_000"
            );

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            _mockPaymentUow.Verify(x => x.CommitAsync(), Times.Never);
        }

        #endregion

        #region HandleRenewalSuccessPayment (Test Logic tính tháng)

        [Fact(DisplayName = "HandleRenewalSuccessPayment: Gia hạn 12 tháng nếu Amount khớp giá phòng")]
        public async Task HandleRenewalSuccessPayment_Extends12Months_WhenAmountMatches()
        {
            // Arrange
            string appTransId = "TRANS_RENEW_12";
            string contractId = "CON_01";
            decimal roomPrice = 2000000;

            var payment = new Payment { TransactionID = appTransId, ReceiptID = "REC_RENEW", Status = "Pending" };
            var receipt = new Receipt { ReceiptID = "REC_RENEW", RelatedObjectID = contractId, Status = "Pending", Amount = roomPrice };

            // Setup Contract & Room
            var contract = new Contract
            {
                ContractID = contractId,
                Room = new Room { RoomType = new RoomType { Price = roomPrice } }
            };

            Mock.Get(_mockPaymentUow.Object.Payments).Setup(x => x.GetByIdAsync(appTransId)).ReturnsAsync(payment);
            Mock.Get(_mockPaymentUow.Object.Receipts).Setup(x => x.GetByIdAsync("REC_RENEW")).ReturnsAsync(receipt);

            Mock.Get(_mockPaymentUow.Object.Contracts).Setup(x => x.GetDetailContractAsync(contractId)).ReturnsAsync(contract);

            _mockContractService.Setup(x => x.ConfirmContractExtensionAsync(contractId, 12))
                .ReturnsAsync((true, "Extended 12 months", 200));

            // Act
            var result = await InvokePrivateMethodAsync<(bool Success, string Message, int StatusCode)>(
                "HandleRenewalSuccessPayment", appTransId, "ZP_RENEW"
            );

            // Assert
            Assert.True(result.Success);
            _mockContractService.Verify(x => x.ConfirmContractExtensionAsync(contractId, 12), Times.Once);
        }

        [Fact(DisplayName = "HandleRenewalSuccessPayment: Gia hạn 6 tháng nếu Amount khác giá phòng")]
        public async Task HandleRenewalSuccessPayment_Extends6Months_WhenAmountDiffers()
        {
            // Arrange
            string appTransId = "TRANS_RENEW_06";
            string contractId = "CON_02";
            decimal roomPrice = 2000000;
            decimal payAmount = 1000000; 

            var payment = new Payment { TransactionID = appTransId, ReceiptID = "REC_RENEW", Status = "Pending" };
            var receipt = new Receipt { ReceiptID = "REC_RENEW", RelatedObjectID = contractId, Status = "Pending", Amount = payAmount };

            var contract = new Contract
            {
                ContractID = contractId,
                Room = new Room { RoomType = new RoomType { Price = roomPrice } }
            };

            Mock.Get(_mockPaymentUow.Object.Payments).Setup(x => x.GetByIdAsync(appTransId)).ReturnsAsync(payment);
            Mock.Get(_mockPaymentUow.Object.Receipts).Setup(x => x.GetByIdAsync("REC_RENEW")).ReturnsAsync(receipt);
            Mock.Get(_mockPaymentUow.Object.Contracts).Setup(x => x.GetDetailContractAsync(contractId)).ReturnsAsync(contract);

            _mockContractService.Setup(x => x.ConfirmContractExtensionAsync(contractId, 6))
                .ReturnsAsync((true, "Extended 6 months", 200));

            // Act
            await InvokePrivateMethodAsync<(bool, string, int)>("HandleRenewalSuccessPayment", appTransId, "ZP_RENEW");

            // Assert
            _mockContractService.Verify(x => x.ConfirmContractExtensionAsync(contractId, 6), Times.Once);
        }

        #endregion

        #region HandleRoomChangeSuccessPayment

        [Fact(DisplayName = "HandleRoomChangeSuccessPayment: Parse CMD lấy RoomID mới thành công")]
        public async Task HandleRoomChangeSuccessPayment_ParsesRoomId_Successfully()
        {
            // Arrange
            string appTransId = "TRANS_CHANGE";
            string contractId = "CON_OLD";
            string oldRoomId = "ROOM_OLD_101";
            string newRoomId = "ROOM_NEW_101";
            var oldRoom = new Room
            {
                RoomID = oldRoomId,
                RoomName = "Old Room",
                CurrentOccupancy = 1,
                Capacity = 4,
                RoomStatus = "Available"
            };
            var newRoom = new Room
            {
                RoomID = newRoomId,
                RoomName = "New Room",
                CurrentOccupancy = 0,
                Capacity = 4,
                RoomStatus = "Available"
            };
            var payment = new Payment { TransactionID = appTransId, ReceiptID = "REC_CHANGE", Status = "Pending" };
            var receipt = new Receipt
            {
                ReceiptID = "REC_CHANGE",
                RelatedObjectID = contractId,
                Status = "Pending",
                Content = $"Fee for changing room CMD|{newRoomId}|SomeOtherInfo"
            };

            var activeContract = new Contract { ContractID = contractId,RoomID=oldRoomId };
            
            Mock.Get(_mockPaymentUow.Object.Payments).Setup(x => x.GetByIdAsync(appTransId)).ReturnsAsync(payment);
            Mock.Get(_mockPaymentUow.Object.Receipts).Setup(x => x.GetByIdAsync("REC_CHANGE")).ReturnsAsync(receipt);

            var mockRoomRepo = new Mock<IRoomRepository>();
            mockRoomRepo.Setup(x => x.GetByIdAsync(oldRoomId)).ReturnsAsync(oldRoom);
            mockRoomRepo.Setup(x => x.GetByIdAsync(newRoomId)).ReturnsAsync(newRoom);

            // Mock _contractUow bên trong hàm này  GetByIdAsync)
            var mockContractRepo = new Mock<IContractRepository>();
            mockContractRepo.Setup(x => x.GetByIdAsync(contractId)).ReturnsAsync(activeContract);
            _mockContractUow.Setup(u => u.Contracts).Returns(mockContractRepo.Object);
            _mockContractUow.Setup(u => u.Rooms).Returns(mockRoomRepo.Object);

            // Act
            var result = await InvokePrivateMethodAsync<(bool Success, string Message, int StatusCode)>(
                "HandleRoomChangeSuccessPayment", appTransId, "ZP_CHANGE"
            );

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Contains("successfull", result.Message);

            // Verify logic đổi phòng đã chạy
            Assert.Equal(newRoomId, activeContract.RoomID); // Hợp đồng đã đổi sang phòng mới
            Assert.Equal(0, oldRoom.CurrentOccupancy);      // Phòng cũ giảm người (1 -> 0)
            Assert.Equal(1, newRoom.CurrentOccupancy);
        }

        #endregion
    }
}