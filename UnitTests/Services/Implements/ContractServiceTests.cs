using API.Hubs;
using API.Services.Implements;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.ConfirmDTOs;
using BusinessObject.DTOs.ContractDTOs;
using BusinessObject.Entities;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Services.Implements
{
    public class ContractServiceTests
    {
        // ============================
        // 1. SETUP & MOCK DEPENDENCIES
        // ============================
        private readonly Mock<IContractUow> _mockUow;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
        private readonly Mock<ILogger<ContractService>> _mockLogger;

        // Mock SignalR
        private readonly Mock<IHubClients> _mockHubClients;
        private readonly Mock<IClientProxy> _mockClientProxy;

        // Mock các Repository con
        private readonly Mock<IStudentRepository> _mockStudentRepo;
        private readonly Mock<IContractRepository> _mockContractRepo;
        private readonly Mock<IReceiptRepository> _mockReceiptRepo;
        private readonly Mock<IViolationRepository> _mockViolationRepo;
        private readonly Mock<IRoomRepository> _mockRoomRepo;
        private readonly Mock<IPaymentRepository> _mockPaymentRepo;
        private readonly Mock<INotificationRepository> _mockNotiRepo;

        private readonly ContractService _service;

        public ContractServiceTests()
        {
            // Init Mocks
            _mockUow = new Mock<IContractUow>();
            _mockEmailService = new Mock<IEmailService>();
            _mockHubContext = new Mock<IHubContext<NotificationHub>>();
            _mockLogger = new Mock<ILogger<ContractService>>();

            _mockStudentRepo = new Mock<IStudentRepository>();
            _mockContractRepo = new Mock<IContractRepository>();
            _mockReceiptRepo = new Mock<IReceiptRepository>();
            _mockViolationRepo = new Mock<IViolationRepository>();
            _mockRoomRepo = new Mock<IRoomRepository>();
            _mockPaymentRepo = new Mock<IPaymentRepository>();
            _mockNotiRepo = new Mock<INotificationRepository>();

            // Link Repos to UoW
            _mockUow.Setup(u => u.Students).Returns(_mockStudentRepo.Object);
            _mockUow.Setup(u => u.Contracts).Returns(_mockContractRepo.Object);
            _mockUow.Setup(u => u.Receipts).Returns(_mockReceiptRepo.Object);
            _mockUow.Setup(u => u.Violations).Returns(_mockViolationRepo.Object);
            _mockUow.Setup(u => u.Rooms).Returns(_mockRoomRepo.Object);
            _mockUow.Setup(u => u.Payments).Returns(_mockPaymentRepo.Object);
            _mockUow.Setup(u => u.Notifications).Returns(_mockNotiRepo.Object);

            // Mock Transaction
            _mockUow.Setup(u => u.BeginTransactionAsync(It.IsAny<System.Data.IsolationLevel>())).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            // Mock SignalR Logic (Để không bị NullRef khi gọi Clients.User)
            _mockHubClients = new Mock<IHubClients>();
            _mockClientProxy = new Mock<IClientProxy>();
            _mockHubContext.Setup(h => h.Clients).Returns(_mockHubClients.Object);
            _mockHubClients.Setup(c => c.User(It.IsAny<string>())).Returns(_mockClientProxy.Object);

            // Inject Service
            _service = new ContractService(_mockUow.Object, _mockEmailService.Object, _mockHubContext.Object, _mockLogger.Object);
        }

        // ==========================================================
        // REQUEST RENEWAL (YÊU CẦU GIA HẠN)
        // ==========================================================

        [Fact(DisplayName = "RequestRenewal: Thành công (Tạo hóa đơn gia hạn)")]
        public async Task RequestRenewal_ShouldSuccess_WhenEligible()
        {
            // Arrange
            string studentId = "S1";
            var student = new Student { StudentID = studentId };
            var room = new Room { RoomID = "R1", RoomType = new RoomType { Price = 1000 } };

            // Hợp đồng sắp hết hạn
            var contract = new Contract
            {
                ContractID = "C1",
                StudentID = studentId,
                Room = room,
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5))
            };

            _mockStudentRepo.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync(student);
            // Mock tìm thấy hợp đồng Active
            _mockContractRepo.Setup(r => r.GetActiveAndNearExpiringContractByStudentId(studentId)).ReturnsAsync(contract);
            // Mock chưa có yêu cầu nào đang pending
            _mockContractRepo.Setup(r => r.HasPendingRenewalRequestAsync(studentId)).ReturnsAsync(false);
            // Mock không có vi phạm
            _mockViolationRepo.Setup(r => r.CountViolationsByStudentId(studentId)).ReturnsAsync(0);

            // Act
            var result = await _service.RequestRenewalAsync(studentId, 6);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);
            Assert.NotNull(result.receiptId);

            // Verify: Phải tạo Receipt trạng thái Pending
            _mockReceiptRepo.Verify(r => r.Add(It.Is<Receipt>(x => x.Status == "Pending" && x.PaymentType == "RenewalContract")), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "RequestRenewal: Thất bại khi sinh viên vi phạm kỷ luật quá nhiều")]
        public async Task RequestRenewal_ShouldFail_WhenTooManyViolations()
        {
            // Arrange
            string studentId = "S_BadBoy";
            var contract = new Contract { ContractID = "C1" };

            _mockStudentRepo.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync(new Student());
            _mockContractRepo.Setup(r => r.GetActiveAndNearExpiringContractByStudentId(studentId)).ReturnsAsync(contract);
            _mockContractRepo.Setup(r => r.HasPendingRenewalRequestAsync(studentId)).ReturnsAsync(false);

            // Mock: Có 3 vi phạm (Giả sử quy định >= 3 là chặn)
            _mockViolationRepo.Setup(r => r.CountViolationsByStudentId(studentId)).ReturnsAsync(3);

            // Act
            var result = await _service.RequestRenewalAsync(studentId, 6);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Too many violations", result.Message);
            _mockUow.Verify(u => u.CommitAsync(), Times.Never); // Không được commit
        }

        // ==========================================================
        // CONFIRM EXTENSION (XÁC NHẬN GIA HẠN)
        // ==========================================================

        [Fact(DisplayName = "ConfirmExtension: Thành công (Cộng ngày kết thúc & Gửi mail)")]
        public async Task ConfirmExtension_ShouldSuccess_WhenReceiptPaid()
        {
            // Arrange
            string contractId = "C1";
            int monthsToAdd = 6;
            var oldEndDate = DateOnly.FromDateTime(DateTime.Now);

            var student = new Student
            {
                FullName = "Nguyen Van A",
                Email = "a@test.com",
                Account = new Account { UserId = "Acc1" }
            };

            var contract = new Contract
            {
                ContractID = contractId,
                EndDate = oldEndDate,
                Student = student,
                Room = new Room { RoomName = "R101", Building = new Building { BuildingName = "B1" } }
            };

            var receipt = new Receipt { Amount = 5000000 };

            _mockContractRepo.Setup(r => r.GetDetailContractAsync(contractId)).ReturnsAsync(contract);
            _mockReceiptRepo.Setup(r => r.GetReceiptByTypeAndRelatedIdAsync(It.IsAny<string>(), contractId)).ReturnsAsync(receipt);

            // Act
            var result = await _service.ConfirmContractExtensionAsync(contractId, monthsToAdd);

            // Assert
            Assert.True(result.Success);

            // 1. Kiểm tra ngày kết thúc đã được cộng thêm chưa
            Assert.Equal(oldEndDate.AddMonths(monthsToAdd), contract.EndDate);

            // 2. Kiểm tra có lưu thông báo không
            _mockUow.Verify(u => u.Notifications.Add(It.IsAny<Notification>()), Times.Once);

            // 3. Kiểm tra có gửi mail không
            _mockEmailService.Verify(e => e.SendRenewalPaymentEmailAsync(It.IsAny<DormRenewalSuccessDto>()), Times.Once);

        }

        // ==========================================================
        // CHỨC NĂNG 3: CHANGE ROOM (ĐỔI PHÒNG - PHỨC TẠP NHẤT)
        // ==========================================================

        [Fact(DisplayName = "ChangeRoom: Thành công (Thu thêm tiền) khi đổi sang phòng VIP")]
        public async Task ChangeRoom_ShouldSuccess_WithCharge_WhenUpgrading()
        {
            // Arrange
            var request = new ChangeRoomRequestDto { StudentId = "S1", NewRoomId = "R_VIP", Reason = ChangeRoomReasonEnum.DormitoryIssue };

            // Phòng cũ rẻ (1 triệu)
            var oldRoom = new Room { RoomID = "R_Normal", RoomType = new RoomType { Price = 1000000 } };
            // Phòng mới đắt (2 triệu)
            var newRoom = new Room { RoomID = "R_VIP", RoomType = new RoomType { Price = 2000000 }, Capacity = 4 };

            var contract = new Contract
            {
                ContractID = "C1",
                RoomID = "R_Normal",
                Room = oldRoom,
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(180)) // Còn hạn
            };

            _mockContractRepo.Setup(r => r.GetActiveContractByStudentId("S1")).ReturnsAsync(contract);
            _mockRoomRepo.Setup(r => r.GetByIdAsync("R_VIP")).ReturnsAsync(newRoom);
            _mockContractRepo.Setup(r => r.CountContractsByRoomIdAndStatus("R_VIP", "Active")).ReturnsAsync(0);

            // Act
            var result = await _service.ChangeRoomAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Charge", result.Type); // Mong đợi hệ thống yêu cầu đóng thêm tiền
            Assert.NotNull(result.ReceiptId);

            // Verify tạo Receipt loại "Charge"
            _mockReceiptRepo.Verify(r => r.Add(It.Is<Receipt>(x => x.PaymentType.Contains("Charge") && x.Amount > 0)), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "ChangeRoom: Thành công (Hoàn tiền) khi đổi phòng do Lỗi KTX")]
        public async Task ChangeRoom_ShouldSuccess_WithRefund_WhenDormitoryIssue()
        {
            // Arrange
            var request = new ChangeRoomRequestDto
            {
                StudentId = "S1",
                NewRoomId = "R_New",
                Reason = ChangeRoomReasonEnum.DormitoryIssue
            };

            var oldRoom = new Room { RoomID = "R_Old", RoomType = new RoomType { Price = 2000000 } };
            var newRoom = new Room { RoomID = "R_New", RoomType = new RoomType { Price = 1000000 }, Capacity = 4 };

            var contract = new Contract
            {
                ContractID = "C1",
                Room = oldRoom,
                RoomID = "R_Old", 
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(180))
            };

            _mockContractRepo.Setup(r => r.GetActiveContractByStudentId("S1")).ReturnsAsync(contract);
            _mockRoomRepo.Setup(r => r.GetByIdAsync("R_New")).ReturnsAsync(newRoom);
            _mockContractRepo.Setup(r => r.CountContractsByRoomIdAndStatus("R_New", "Active")).ReturnsAsync(0);
            _mockRoomRepo.Setup(r => r.GetByIdAsync("R_Old")).ReturnsAsync(oldRoom);

            // Act
            var result = await _service.ChangeRoomAsync(request);

            // Assert
            Assert.True(result.Success, $"Failed with message: {result.Message}"); 
            Assert.Equal("Refund", result.Type);

            _mockReceiptRepo.Verify(r => r.Add(It.Is<Receipt>(x => x.PaymentType.Contains("Refund"))), Times.Once);
            _mockPaymentRepo.Verify(p => p.Add(It.Is<Payment>(x => x.Status == "Success")), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "ChangeRoom: Thất bại khi phòng mới đã đầy")]
        public async Task ChangeRoom_ShouldFail_WhenNewRoomFull()
        {
            // Arrange
            var request = new ChangeRoomRequestDto { StudentId = "S1", NewRoomId = "R_Full" };

            var contract = new Contract { ContractID = "C1", Room = new Room() };
            var newRoom = new Room { RoomID = "R_Full", Capacity = 4 };

            _mockContractRepo.Setup(r => r.GetActiveContractByStudentId("S1")).ReturnsAsync(contract);
            _mockRoomRepo.Setup(r => r.GetByIdAsync("R_Full")).ReturnsAsync(newRoom);

            // Mock: Phòng đã có 4 người
            _mockContractRepo.Setup(r => r.CountContractsByRoomIdAndStatus("R_Full", "Active")).ReturnsAsync(4);

            // Act
            var result = await _service.ChangeRoomAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("full", result.Message.ToLower());
        }

        // ==========================================================
        // CHỨC NĂNG 4: TERMINATE CONTRACT (CHẤM DỨT HỢP ĐỒNG)
        // ==========================================================

        [Fact(DisplayName = "TerminateContract: Thành công (Cập nhật trạng thái & Trả slot phòng)")]
        public async Task TerminateContract_ShouldSuccess_WhenValid()
        {
            // Arrange
            string studentId = "S1";

            // Phòng đang full (4/4)
            var room = new Room { RoomID = "R1", CurrentOccupancy = 4, RoomStatus = "Full" };
            var contract = new Contract
            {
                ContractID = "C1",
                StudentID = studentId,
                ContractStatus = "Active",
                Room = room
            };

            _mockContractRepo.Setup(r => r.GetActiveContractByStudentId(studentId)).ReturnsAsync(contract);

            // Act
            var result = await _service.TerminateContractNowAsync(studentId);

            // Assert
            Assert.True(result.Success);

            // 1. Contract status -> Terminated
            Assert.Equal("Terminated", contract.ContractStatus);
            Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), contract.EndDate);

            // 2. Room Occupancy giảm 1 -> 3
            Assert.Equal(3, room.CurrentOccupancy);
            Assert.Equal("Available", room.RoomStatus); // Từ Full -> Available

            _mockContractRepo.Verify(r => r.Update(contract), Times.Once);
            _mockRoomRepo.Verify(r => r.Update(room), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }
    }
}