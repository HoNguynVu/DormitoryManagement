using API.Services.Implements;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.Config;
using BusinessObject.DTOs.PaymentDTOs;
using BusinessObject.Entities;
using DataAccess.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;
using Xunit;

namespace UnitTests.Services.Implements
{
    public class PaymentServiceTests
    {
        private readonly Mock<IOptions<ZaloPaySettings>> _mockOptions;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<IPaymentUow> _mockPaymentUow;
        private readonly Mock<IContractUow> _mockContractUow;
        private readonly Mock<ILogger<PaymentService>> _mockLogger;

        // Mock các service phụ thuộc khác
        private readonly Mock<IHealthInsuranceService> _mockHealthService;
        private readonly Mock<IRegistrationService> _mockRegisService;
        private readonly Mock<IContractService> _mockContractService;
        private readonly Mock<IMaintenanceService> _mockMaintenanceService;
        private readonly Mock<IUtilityBillService> _mockUtilityService;

        private readonly PaymentService _service;
        private readonly ZaloPaySettings _zaloSettings;

        public PaymentServiceTests()
        {
            // 1. Setup Config
            _zaloSettings = new ZaloPaySettings
            {
                AppId = "123",
                Key1 = "key1_secret",
                Key2 = "key2_secret",
                CreateOrderUrl = "https://sb-openapi.zalopay.vn/v2/create",
            };
            _mockOptions = new Mock<IOptions<ZaloPaySettings>>();
            _mockOptions.Setup(x => x.Value).Returns(_zaloSettings);

            // 2. Setup UoW & Repositories
            _mockPaymentUow = new Mock<IPaymentUow>();
            _mockContractUow = new Mock<IContractUow>();

            _mockPaymentUow.Setup(u => u.RegistrationForms).Returns(new Mock<IRegistrationFormRepository>().Object);
            _mockPaymentUow.Setup(u => u.RoomTypes).Returns(new Mock<IRoomTypeRepository>().Object);
            _mockPaymentUow.Setup(u => u.Receipts).Returns(new Mock<IReceiptRepository>().Object);
            _mockPaymentUow.Setup(u => u.Payments).Returns(new Mock<IPaymentRepository>().Object);
            _mockPaymentUow.Setup(u => u.Students).Returns(new Mock<IStudentRepository>().Object);
            _mockPaymentUow.Setup(u => u.UtilityBills).Returns(new Mock<IUtilityBillRepository>().Object);
            _mockPaymentUow.Setup(u => u.Contracts).Returns(new Mock<IContractRepository>().Object);

            // 3. Setup Logger & HttpClient
            _mockLogger = new Mock<ILogger<PaymentService>>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();

            // 4. Setup Services khác
            _mockHealthService = new Mock<IHealthInsuranceService>();
            _mockRegisService = new Mock<IRegistrationService>();
            _mockContractService = new Mock<IContractService>();
            _mockMaintenanceService = new Mock<IMaintenanceService>();
            _mockUtilityService = new Mock<IUtilityBillService>();

            // Init Service
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

        #region Helper: Mock ZaloPay API Response
        private void MockZaloPaySuccessResponse(string orderUrl = "https://zalopay.vn/pay")
        {
            var responseData = new
            {
                return_code = 1,
                order_url = orderUrl,
                zp_trans_token = "token123"
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(responseData))
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);
        }
        #endregion

        #region CreateZaloPayLinkForRegistration Tests

        [Fact(DisplayName = "Tạo link thanh toán đăng ký: Thành công (200) khi dữ liệu hợp lệ")]
        public async Task CreateZaloPayLinkForRegistration_Returns200_WhenValid()
        {
            // Arrange
            string regisId = "REG01";
            var form = new RegistrationForm { FormID = regisId, Status = "Pending", RoomID = "R101", StudentID = "S01" };
            var roomType = new RoomType { RoomTypeID = "RT01", Price = 500000 };

            var mockRegisRepo = Mock.Get(_mockPaymentUow.Object.RegistrationForms);
            mockRegisRepo.Setup(x => x.GetByIdAsync(regisId)).ReturnsAsync(form);

            var mockRoomTypeRepo = Mock.Get(_mockPaymentUow.Object.RoomTypes);
            mockRoomTypeRepo.Setup(x => x.GetRoomTypeByRoomId("R101")).ReturnsAsync(roomType);

            MockZaloPaySuccessResponse();

            // Act
            var result = await _service.CreateZaloPayLinkForRegistration(regisId);

            // Assert
            Assert.Equal(200, result.StatusCode);
            Assert.True(result.dto.IsSuccess);
            _mockPaymentUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "Tạo link thanh toán đăng ký: Thất bại (404) khi không tìm thấy đơn đăng ký")]
        public async Task CreateZaloPayLinkForRegistration_Returns404_WhenFormNotFound()
        {
            // Arrange
            var mockRegisRepo = Mock.Get(_mockPaymentUow.Object.RegistrationForms);
            mockRegisRepo.Setup(x => x.GetByIdAsync("INVALID")).ReturnsAsync((RegistrationForm)null);

            // Act
            var result = await _service.CreateZaloPayLinkForRegistration("INVALID");

            // Assert
            Assert.Equal(404, result.StatusCode);
            Assert.False(result.dto.IsSuccess);
        }

        [Fact(DisplayName = "Tạo link thanh toán đăng ký: Thất bại (400) khi trạng thái không phải Pending")]
        public async Task CreateZaloPayLinkForRegistration_Returns400_WhenStatusNotPending()
        {
            // Arrange
            var form = new RegistrationForm { FormID = "R1", Status = "Approved" };
            var mockRegisRepo = Mock.Get(_mockPaymentUow.Object.RegistrationForms);
            mockRegisRepo.Setup(x => x.GetByIdAsync("R1")).ReturnsAsync(form);

            // Act
            var result = await _service.CreateZaloPayLinkForRegistration("R1");

            // Assert
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("not in pending", result.dto.Message);
        }

        #endregion

        #region CreateZaloPayLinkForUtility Tests

        [Fact(DisplayName = "Tạo link thanh toán điện nước: Thành công (200) khi sinh viên ở đúng phòng")]
        public async Task CreateZaloPayLinkForUtility_Returns200_WhenValid()
        {
            // Arrange
            string utilityId = "UTIL01";
            string accountId = "ACC01";
            string studentId = "STU01";
            string roomId = "ROOM101";

            var student = new Student { StudentID = studentId, AccountID = accountId };
            var bill = new UtilityBill { BillID = utilityId, Status = "Unpaid", RoomID = roomId, Amount = 100000 };
            var contract = new Contract { StudentID = studentId, RoomID = roomId, ContractStatus = "Active" };

            var mockStudentRepo = Mock.Get(_mockPaymentUow.Object.Students);
            mockStudentRepo.Setup(x => x.GetStudentByAccountIdAsync(accountId)).ReturnsAsync(student);

            var mockBillRepo = Mock.Get(_mockPaymentUow.Object.UtilityBills);
            mockBillRepo.Setup(x => x.GetByIdAsync(utilityId)).ReturnsAsync(bill);

            var mockContractRepo = Mock.Get(_mockPaymentUow.Object.Contracts);
            mockContractRepo.Setup(x => x.GetActiveContractByStudentId(studentId)).ReturnsAsync(contract);

            MockZaloPaySuccessResponse();

            // Act
            var result = await _service.CreateZaloPayLinkForUtility(utilityId, accountId);

            // Assert
            Assert.Equal(200, result.StatusCode);
            Assert.True(result.dto.IsSuccess);
        }

        [Fact(DisplayName = "Tạo link thanh toán điện nước: Thất bại (403) khi sinh viên không sống tại phòng của hóa đơn")]
        public async Task CreateZaloPayLinkForUtility_Returns403_WhenRoomMismatch()
        {
            // Arrange
            var student = new Student { StudentID = "S1" };
            var bill = new UtilityBill { RoomID = "ROOM_A", Status = "Unpaid" };
            var contract = new Contract { RoomID = "ROOM_B" }; // Khác phòng

            var mockStudentRepo = Mock.Get(_mockPaymentUow.Object.Students);
            mockStudentRepo.Setup(x => x.GetStudentByAccountIdAsync(It.IsAny<string>())).ReturnsAsync(student);

            var mockBillRepo = Mock.Get(_mockPaymentUow.Object.UtilityBills);
            mockBillRepo.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(bill);

            var mockContractRepo = Mock.Get(_mockPaymentUow.Object.Contracts);
            mockContractRepo.Setup(x => x.GetActiveContractByStudentId(It.IsAny<string>())).ReturnsAsync(contract);

            // Act
            var result = await _service.CreateZaloPayLinkForUtility("U1", "A1");

            // Assert
            Assert.Equal(403, result.StatusCode);
            Assert.Contains("do not live in the room", result.dto.Message);
        }

        #endregion

        #region CreateZaloPayLinkForRenewal Tests

        [Fact(DisplayName = "Tạo link gia hạn hợp đồng: Thành công (200) khi hóa đơn hợp lệ")]
        public async Task CreateZaloPayLinkForRenewal_Returns200_WhenValid()
        {
            // Arrange
            string receiptId = "RE01";
            var receipt = new Receipt
            {
                ReceiptID = receiptId,
                Status = "Pending",
                PaymentType = "RenewalContract", 
                Amount = 200000
            };

            var mockReceiptRepo = Mock.Get(_mockPaymentUow.Object.Receipts);
            mockReceiptRepo.Setup(x => x.GetByIdAsync(receiptId)).ReturnsAsync(receipt);

            MockZaloPaySuccessResponse();

            // Act
            var result = await _service.CreateZaloPayLinkForRenewal(receiptId);

            // Assert
            Assert.Equal(200, result.StatusCode);
        }

        [Fact(DisplayName = "Tạo link gia hạn hợp đồng: Thất bại (400) khi sai loại hóa đơn (PaymentType)")]
        public async Task CreateZaloPayLinkForRenewal_Returns400_WhenWrongType()
        {
            // Arrange
            var receipt = new Receipt
            {
                ReceiptID = "R1",
                Status = "Pending",
                PaymentType = "Registration" // Sai loại
            };

            var mockReceiptRepo = Mock.Get(_mockPaymentUow.Object.Receipts);
            mockReceiptRepo.Setup(x => x.GetByIdAsync("R1")).ReturnsAsync(receipt);

            // Act
            var result = await _service.CreateZaloPayLinkForRenewal("R1");

            // Assert
            Assert.Equal(400, result.StatusCode);
        }

        #endregion

        #region ProcessZaloPayCallback Tests

        [Fact(DisplayName = "Xử lý Callback: Trả về -1 khi chữ ký MAC không hợp lệ")]
        public async Task ProcessZaloPayCallback_ReturnsMinus1_WhenMacInvalid()
        {
            // Arrange
            var cbData = new ZaloPayCallbackDTO
            {
                Data = "{\"some\":\"json\"}",
                Mac = "INVALID_MAC"
            };

            // Act
            var result = await _service.ProcessZaloPayCallback(cbData);

            // Assert
            Assert.Equal(-1, result.ReturnCode);
        }

        [Fact(DisplayName = "Xử lý Callback: Trả về 0 khi dữ liệu JSON bị lỗi")]
        public async Task ProcessZaloPayCallback_Returns0_WhenJsonInvalid()
        {
            // Arrange
            string badJson = "Not A Json";
            string validMac = API.Services.Helpers.ZaloPayHelper.HmacSHA256(badJson, _zaloSettings.Key2);

            var cbData = new ZaloPayCallbackDTO
            {
                Data = badJson,
                Mac = validMac
            };

            // Act
            var result = await _service.ProcessZaloPayCallback(cbData);

            // Assert
            Assert.Equal(0, result.ReturnCode);
        }

        #endregion

        #region GetPaymentResultByAppTransId Tests

        [Fact(DisplayName = "Lấy kết quả thanh toán: Thành công (200) khi tìm thấy giao dịch")]
        public async Task GetPaymentResultByAppTransId_Returns200_WhenFound()
        {
            // Arrange
            string transId = "TRANS_01";
            var receipt = new Receipt { ReceiptID = "R1", Amount = 100 };
            var payment = new Payment { TransactionID = transId, PaymentDate = DateTime.Now };

            var mockReceiptRepo = Mock.Get(_mockPaymentUow.Object.Receipts);
            mockReceiptRepo.Setup(x => x.GetReceiptAndDateAsync(transId))
                .ReturnsAsync((receipt, payment));

            // Act
            var result = await _service.GetPaymentResultByAppTransId(transId);

            // Assert
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.dto);
        }

        [Fact(DisplayName = "Lấy kết quả thanh toán: Thất bại (404) khi không tìm thấy giao dịch")]
        public async Task GetPaymentResultByAppTransId_Returns404_WhenNotFound()
        {
            // Arrange
            var mockReceiptRepo = Mock.Get(_mockPaymentUow.Object.Receipts);
            mockReceiptRepo.Setup(x => x.GetReceiptAndDateAsync(It.IsAny<string>()))
                .ReturnsAsync((null, null));

            // Act
            var result = await _service.GetPaymentResultByAppTransId("MISSING");

            // Assert
            Assert.Equal(404, result.StatusCode);
        }

        #endregion
    }
}