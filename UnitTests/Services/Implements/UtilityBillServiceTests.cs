using API.Hubs;
using API.Services.Common;
using API.Services.Implements;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.ConfirmDTOs;
using BusinessObject.DTOs.UtilityBillDTOs;
using BusinessObject.Entities;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Services.Implements
{
    public class UtilityBillServiceTests
    {
        // ==========================================
        // SETUP & MOCK OBJECTS
        // ==========================================
        private readonly Mock<IUtilityBillUow> _mockUow;
        private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ILogger<UtilityBillService>> _mockLogger;
        private readonly UtilityBillService _service;

        // Mock SignalR
        private readonly Mock<IHubClients> _mockHubClients;
        private readonly Mock<IClientProxy> _mockClientProxy;

        // Mock Repositories con
        private readonly Mock<IUtilityBillRepository> _mockBillRepo;
        private readonly Mock<IContractRepository> _mockContractRepo;
        private readonly Mock<IStudentRepository> _mockStudentRepo;
        private readonly Mock<IAccountRepository> _mockAccountRepo;
        private readonly Mock<IParameterRepository> _mockParamRepo;
        private readonly Mock<IReceiptRepository> _mockReceiptRepo;
        private readonly Mock<INotificationRepository> _mockNotiRepo;
        private readonly Mock<IBuildingManagerRepository> _mockManagerRepo;
        private readonly Mock<IRoomRepository> _mockRoomRepo;

        public UtilityBillServiceTests()
        {
            // Init Mocks
            _mockUow = new Mock<IUtilityBillUow>();
            _mockHubContext = new Mock<IHubContext<NotificationHub>>();
            _mockEmailService = new Mock<IEmailService>();
            _mockLogger = new Mock<ILogger<UtilityBillService>>();

            // Init Repo Mocks
            _mockBillRepo = new Mock<IUtilityBillRepository>();
            _mockContractRepo = new Mock<IContractRepository>();
            _mockStudentRepo = new Mock<IStudentRepository>();
            _mockAccountRepo = new Mock<IAccountRepository>();
            _mockParamRepo = new Mock<IParameterRepository>();
            _mockReceiptRepo = new Mock<IReceiptRepository>();
            _mockNotiRepo = new Mock<INotificationRepository>();
            _mockManagerRepo = new Mock<IBuildingManagerRepository>();
            _mockRoomRepo = new Mock<IRoomRepository>();

            // Setup UOW trả về Repo Mocks
            _mockUow.Setup(u => u.UtilityBills).Returns(_mockBillRepo.Object);
            _mockUow.Setup(u => u.Contracts).Returns(_mockContractRepo.Object);
            _mockUow.Setup(u => u.Students).Returns(_mockStudentRepo.Object);
            _mockUow.Setup(u => u.Accounts).Returns(_mockAccountRepo.Object);
            _mockUow.Setup(u => u.Parameters).Returns(_mockParamRepo.Object);
            _mockUow.Setup(u => u.Receipts).Returns(_mockReceiptRepo.Object);
            _mockUow.Setup(u => u.Notifications).Returns(_mockNotiRepo.Object);
            _mockUow.Setup(u => u.BuildingManagers).Returns(_mockManagerRepo.Object);
            _mockUow.Setup(u => u.Rooms).Returns(_mockRoomRepo.Object);

            // Mock Transaction
            _mockUow.Setup(u => u.BeginTransactionAsync(It.IsAny<System.Data.IsolationLevel>())).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

            // Mock SignalR Structure
            _mockHubClients = new Mock<IHubClients>();
            _mockClientProxy = new Mock<IClientProxy>();
            _mockHubContext.Setup(h => h.Clients).Returns(_mockHubClients.Object);
            _mockHubClients.Setup(c => c.User(It.IsAny<string>())).Returns(_mockClientProxy.Object);

            // Inject Service
            _service = new UtilityBillService(_mockUow.Object, _mockHubContext.Object, _mockEmailService.Object, _mockLogger.Object);
        }

        // ==========================================
        // CREATE BILL 
        // ==========================================

        [Fact(DisplayName = "CreateBill: Thất bại - Hóa đơn tháng này của phòng đã tồn tại")]
        public async Task CreateBill_ShouldFail_WhenBillExists()
        {
            // Arrange
            var dto = new CreateBillDTO { RoomId = "R1" };
            _mockBillRepo.Setup(r => r.IsBillExistsAsync("R1", It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);

            // Act
            var result = await _service.CreateUtilityBill(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Bill already exists", result.Message);
        }

        [Fact(DisplayName = "CreateBill: Thất bại - Chỉ số điện/nước mới nhỏ hơn chỉ số cũ")]
        public async Task CreateBill_ShouldFail_WhenIndexInvalid()
        {
            // Arrange
            var dto = new CreateBillDTO { RoomId = "R1", ElectricityIndex = 90, WaterIndex = 90 };
            var lastBill = new UtilityBill { ElectricityNewIndex = 100, WaterNewIndex = 100 }; // Cũ > Mới => Lỗi

            _mockBillRepo.Setup(r => r.IsBillExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);
            _mockBillRepo.Setup(r => r.GetLastMonthBillAsync("R1")).ReturnsAsync(lastBill);

            // Act
            var result = await _service.CreateUtilityBill(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("cannot be less than", result.Message);
        }

        [Fact(DisplayName = "CreateBill: Thành công - Tạo hóa đơn mới & Gửi thông báo cho SV trong phòng")]
        public async Task CreateBill_ShouldSuccess_WhenValid()
        {
            // Arrange
            var dto = new CreateBillDTO { RoomId = "R1", ElectricityIndex = 150, WaterIndex = 120 };

            // Hóa đơn cũ: Điện 100, Nước 100 -> Dùng: 50, 20
            var lastBill = new UtilityBill { ElectricityNewIndex = 100, WaterNewIndex = 100 };

            var parameter = new Parameter { DefaultElectricityPrice = 3000, DefaultWaterPrice = 5000 };

            // 2 sinh viên đang ở trong phòng
            var contracts = new List<Contract>
            {
                new Contract { Student = new Student { AccountID = "Acc1" } },
                new Contract { Student = new Student { AccountID = "Acc2" } }
            };

            _mockBillRepo.Setup(r => r.IsBillExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);
            _mockBillRepo.Setup(r => r.GetLastMonthBillAsync("R1")).ReturnsAsync(lastBill);
            _mockParamRepo.Setup(r => r.GetActiveParameterAsync()).ReturnsAsync(parameter);
            _mockContractRepo.Setup(r => r.GetContractsByRoomIdAndStatus("R1", "Active")).ReturnsAsync(contracts);

            // Act
            var result = await _service.CreateUtilityBill(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);

            // Verify: Đã thêm Bill với tính toán đúng
            _mockBillRepo.Verify(r => r.Add(It.Is<UtilityBill>(b =>
                b.ElectricityUsage == 50 &&
                b.WaterUsage == 20 &&
                b.Amount == (50 * 3000) + (20 * 5000) && // 150k + 100k = 250k
                b.Status == "Unpaid"
            )), Times.Once);

            // Verify: Đã lưu thông báo (cho 2 SV)
            _mockNotiRepo.Verify(r => r.Add(It.IsAny<Notification>()), Times.Exactly(2));

            // Verify: Đã bắn SignalR (cho 2 SV)
            _mockHubClients.Verify(c => c.User("Acc1"), Times.Once);
            _mockHubClients.Verify(c => c.User("Acc2"), Times.Once);

            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        // ==========================================
        // CONFIRM PAYMENT
        // ==========================================

        [Fact(DisplayName = "ConfirmPayment: Thất bại - Hóa đơn không tồn tại hoặc đã thanh toán (Paid)")]
        public async Task ConfirmPayment_ShouldFail_WhenInvalidStatus()
        {
            // Arrange
            var paidBill = new UtilityBill { BillID = "B1", Status = "Paid" };
            _mockBillRepo.Setup(r => r.GetByIdAsync("B1")).ReturnsAsync(paidBill);

            // Act
            var result = await _service.ConfirmUtilityPaymentAsync("B1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Bill is already paid", result.Message);
        }

        [Fact(DisplayName = "ConfirmPayment: Thành công - Cập nhật trạng thái 'Paid', cập nhật biên lai và gửi Email xác nhận")]
        public async Task ConfirmPayment_ShouldSuccess_WhenPending()
        {
            // Arrange
            string billId = "B1";
            var bill = new UtilityBill
            {
                BillID = billId,
                Status = "Unpaid",
                Month = 10,
                Year = 2025,
                Room = new Room { RoomName = "R101", Building = new Building { BuildingName = "Block A" } },
                Amount = 500000
            };

            var student = new Student
            {
                StudentID = "S1",
                Email = "s1@test.com",
                FullName = "Nguyen Van A",
                Account = new Account { UserId = "Acc1" }
            };

            var receipt = new Receipt
            {
                ReceiptID = "Rec1",
                Student = student,
                Status = "Pending"
            };

            var parameter = new Parameter { DefaultElectricityPrice = 3000, DefaultWaterPrice = 5000 };

            _mockBillRepo.Setup(r => r.GetByIdAsync(billId)).ReturnsAsync(bill);
            _mockReceiptRepo.Setup(r => r.GetReceiptByTypeAndRelatedIdAsync("Utility", billId)).ReturnsAsync(receipt);
            _mockStudentRepo.Setup(r => r.GetByIdAsync(student.StudentID)).ReturnsAsync(student);
            _mockParamRepo.Setup(r => r.GetActiveParameterAsync()).ReturnsAsync(parameter);

            // Act
            var result = await _service.ConfirmUtilityPaymentAsync(billId);

            // Assert
            Assert.True(result.Success);

            // Verify: Cập nhật trạng thái
            Assert.Equal("Paid", bill.Status);
            Assert.Equal("Success", receipt.Status);

            _mockBillRepo.Verify(r => r.Update(bill), Times.Once);
            _mockReceiptRepo.Verify(r => r.Update(receipt), Times.Once);
            _mockNotiRepo.Verify(r => r.Add(It.IsAny<Notification>()), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);

            // Verify: Gửi Email
            _mockEmailService.Verify(e => e.SendUtilityPaymentEmailAsync(It.Is<UtilityPaymentSuccessDto>(
                dto => dto.StudentEmail == "s1@test.com" && dto.TotalAmount == 500000
            )), Times.Once);
        }

        // ==========================================
        // GET BILLS FOR MANAGER
        // ==========================================

        [Fact(DisplayName = "GetBillsForManager: Thất bại - Không tìm thấy tài khoản quản lý")]
        public async Task GetBillsForManager_ShouldFail_WhenManagerMissing()
        {
            // Arrange
            var req = new ManagerGetBillRequest { AccountId = "Acc_Mgr" };
            _mockManagerRepo.Setup(r => r.GetByAccountIdAsync(req.AccountId)).ReturnsAsync((BuildingManager)null);

            // Act
            var result = await _service.GetBillsForManagerAsync(req);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact(DisplayName = "GetBillsForManager: Thành công - Trả về danh sách (Bao gồm cả phòng chưa có hóa đơn)")]
        public async Task GetBillsForManager_ShouldReturnMixedList()
        {
            // Arrange
            var req = new ManagerGetBillRequest { AccountId = "Acc_Mgr", Month = 10, Year = 2025 };
            var manager = new BuildingManager { ManagerID = "M1" };

            var rooms = new List<Room>
            {
                new Room { RoomID = "R1", RoomName = "Room 1" }, // Có hóa đơn
                new Room { RoomID = "R2", RoomName = "Room 2" }  // Chưa có
            };

            var billR1 = new UtilityBill { RoomID = "R1", Amount = 100000, Status = "Paid" };

            _mockManagerRepo.Setup(r => r.GetByAccountIdAsync(req.AccountId)).ReturnsAsync(manager);
            _mockRoomRepo.Setup(r => r.GetRoomByManagerIdAsync("M1")).ReturnsAsync(rooms);

            // Mock bills
            _mockBillRepo.Setup(r => r.GetByRoomAndPeriodAsync("R1", 10, 2025)).ReturnsAsync(billR1);
            _mockBillRepo.Setup(r => r.GetByRoomAndPeriodAsync("R2", 10, 2025)).ReturnsAsync((UtilityBill)null);

            // Act
            var result = await _service.GetBillsForManagerAsync(req);

            // Assert
            Assert.True(result.Success);
            var list = result.listBill.ToList();
            Assert.Equal(2, list.Count);

            // Check Phòng 1 (Có hóa đơn)
            var dto1 = list.First(x => x.RoomID == "R1");
            Assert.Equal(100000, dto1.Amount);
            Assert.Equal("Paid", dto1.Status);

            // Check Phòng 2 (Không có hóa đơn)
            var dto2 = list.First(x => x.RoomID == "R2");
            Assert.Equal("No Bill", dto2.Status);
        }

        // ==========================================
        // GET BILLS BY STUDENT
        // ==========================================

        [Fact(DisplayName = "GetBillsByStudent: Thất bại - Sinh viên không có hợp đồng đang hoạt động")]
        public async Task GetBillsByStudent_ShouldFail_WhenNoContract()
        {
            // Arrange
            string accId = "Acc_S1";
            var account = new Account { Email = "s1@test.com" };
            var student = new Student { StudentID = "S1" };

            _mockAccountRepo.Setup(r => r.GetByIdAsync(accId)).ReturnsAsync(account);
            _mockStudentRepo.Setup(r => r.GetStudentByEmailAsync(account.Email)).ReturnsAsync(student);
            _mockContractRepo.Setup(r => r.GetActiveAndNearExpiringContractByStudentId("S1")).ReturnsAsync((Contract)null);

            // Act
            var result = await _service.GetUtilityBillsByStudent(accId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("No active contract", result.Message);
        }

        [Fact(DisplayName = "GetBillsByStudent: Thành công - Lấy danh sách hóa đơn theo phòng của SV")]
        public async Task GetBillsByStudent_ShouldReturnList()
        {
            // Arrange
            string accId = "Acc_S1";
            var account = new Account { Email = "s1@test.com" };
            var student = new Student { StudentID = "S1" };
            var contract = new Contract { RoomID = "R1" };
            var bills = new List<UtilityBill>
            {
                new UtilityBill { BillID = "B1", Month = 10 },
                new UtilityBill { BillID = "B2", Month = 9 }
            };

            _mockAccountRepo.Setup(r => r.GetByIdAsync(accId)).ReturnsAsync(account);
            _mockStudentRepo.Setup(r => r.GetStudentByEmailAsync(account.Email)).ReturnsAsync(student);
            _mockContractRepo.Setup(r => r.GetActiveAndNearExpiringContractByStudentId("S1")).ReturnsAsync(contract);
            _mockBillRepo.Setup(r => r.GetByRoomAsync("R1")).ReturnsAsync(bills);

            // Act
            var result = await _service.GetUtilityBillsByStudent(accId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.listBill.Count());
        }
    }
}