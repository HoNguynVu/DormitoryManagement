using API.Hubs;
using API.Services.Implements;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.ConfirmDTOs;
using BusinessObject.DTOs.ViolationDTOs;
using BusinessObject.Entities;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Services.Implements
{
    public class ViolationServiceTests
    {
        // ==========================================
        // SETUP & MOCK OBJECTS
        // ==========================================
        private readonly Mock<IViolationUow> _mockUow;
        private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly ViolationService _service;

        // Mock SignalR 
        private readonly Mock<IHubClients> _mockHubClients;
        private readonly Mock<IClientProxy> _mockClientProxy;

        // Mock các Repository con
        private readonly Mock<IBuildingManagerRepository> _mockManagerRepo;
        private readonly Mock<IViolationRepository> _mockViolationRepo;
        private readonly Mock<IAccountRepository> _mockAccountRepo;
        private readonly Mock<INotificationRepository> _mockNotificationRepo;
        private readonly Mock<IContractRepository> _mockContractRepo;
        private readonly Mock<IStudentRepository> _mockStudentRepo;

        public ViolationServiceTests()
        {
            // Init Top-level Mocks
            _mockUow = new Mock<IViolationUow>();
            _mockHubContext = new Mock<IHubContext<NotificationHub>>();
            _mockEmailService = new Mock<IEmailService>();

            // Init Repo Mocks
            _mockManagerRepo = new Mock<IBuildingManagerRepository>();
            _mockViolationRepo = new Mock<IViolationRepository>();
            _mockAccountRepo = new Mock<IAccountRepository>();
            _mockNotificationRepo = new Mock<INotificationRepository>();
            _mockContractRepo = new Mock<IContractRepository>();
            _mockStudentRepo = new Mock<IStudentRepository>();

            // Link Repo to UOW
            _mockUow.Setup(u => u.BuildingManagers).Returns(_mockManagerRepo.Object);
            _mockUow.Setup(u => u.Violations).Returns(_mockViolationRepo.Object);
            _mockUow.Setup(u => u.Accounts).Returns(_mockAccountRepo.Object);
            _mockUow.Setup(u => u.Notifications).Returns(_mockNotificationRepo.Object);
            _mockUow.Setup(u => u.Contracts).Returns(_mockContractRepo.Object);
            _mockUow.Setup(u => u.Students).Returns(_mockStudentRepo.Object);

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
            _service = new ViolationService(_mockUow.Object, _mockHubContext.Object, _mockEmailService.Object);
        }

        // ==========================================
        // CREATE VIOLATION
        // ==========================================

        [Fact(DisplayName = "CreateViolation: Thất bại (404) khi không tìm thấy quản lý tòa nhà")]
        public async Task CreateViolation_ShouldFail_WhenManagerNotFound()
        {
            // Arrange
            var request = new CreateViolationRequest { AccountId = "Acc_Unknown" };
            _mockManagerRepo.Setup(r => r.GetByAccountIdAsync(request.AccountId)).ReturnsAsync((BuildingManager)null);

            // Act
            var result = await _service.CreateViolationAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Building manager not found.", result.Message);
        }

        [Fact(DisplayName = "CreateViolation: Thất bại (404) khi không tìm thấy tài khoản sinh viên")]
        public async Task CreateViolation_ShouldFail_WhenStudentAccountNotFound()
        {
            // Arrange
            var request = new CreateViolationRequest { AccountId = "Acc_Mgr", StudentId = "S_Ghost" };
            _mockManagerRepo.Setup(r => r.GetByAccountIdAsync("Acc_Mgr")).ReturnsAsync(new BuildingManager());
            _mockAccountRepo.Setup(r => r.GetAccountByStudentId("S_Ghost")).ReturnsAsync((Account)null);

            // Act
            var result = await _service.CreateViolationAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("Account not found", result.Message);
        }

        [Fact(DisplayName = "CreateViolation: Thành công (Lần 1) - Chỉ tạo Vi phạm & Thông báo")]
        public async Task CreateViolation_ShouldSuccess_NormalCase()
        {
            // Arrange
            var request = new CreateViolationRequest
            {
                AccountId = "Acc_Mgr",
                StudentId = "S1",
                ViolationAct = "Hút thuốc",
                Description = "Tại hành lang"
            };

            var manager = new BuildingManager { ManagerID = "M1" };
            var account = new Account { UserId = "Acc_S1" }; // Account của SV để gửi noti

            _mockManagerRepo.Setup(r => r.GetByAccountIdAsync(request.AccountId)).ReturnsAsync(manager);
            _mockAccountRepo.Setup(r => r.GetAccountByStudentId(request.StudentId)).ReturnsAsync(account);

            // Mock: Mới vi phạm lần 1 (chưa đủ 3)
            _mockViolationRepo.Setup(r => r.CountViolationsByStudentId(request.StudentId)).ReturnsAsync(1);

            // Mock: Lấy lại violation sau khi add (để trả về response)
            var newViolation = new Violation { ViolationID = "V_New", StudentID = "S1", ViolationAct = "Hút thuốc" };
            _mockViolationRepo.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(newViolation);

            // Act
            var result = await _service.CreateViolationAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal("Hút thuốc", result.Data.ViolationAct);

            // Verify
            _mockViolationRepo.Verify(r => r.Add(It.IsAny<Violation>()), Times.Once);
            _mockNotificationRepo.Verify(r => r.Add(It.IsAny<Notification>()), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);

            // Đảm bảo KHÔNG gọi logic chấm dứt hợp đồng
            _mockContractRepo.Verify(r => r.Update(It.IsAny<Contract>()), Times.Never);
        }

        [Fact(DisplayName = "CreateViolation: Thành công (Lần 3) - Tự động CHẤM DỨT HỢP ĐỒNG & Gửi Mail")]
        public async Task CreateViolation_ShouldTerminateContract_When3rdViolation()
        {
            // Arrange
            var request = new CreateViolationRequest { AccountId = "Acc_Mgr", StudentId = "S_BadBoy" };
            var manager = new BuildingManager { ManagerID = "M1" };
            var account = new Account { UserId = "Acc_S1" };

            // Mock hợp đồng đang Active
            var activeContract = new Contract
            {
                ContractID = "C1",
                ContractStatus = "Active",
                StudentID = "S_BadBoy",
                Student = new Student { FullName = "Nguyen Van A", Email = "a@test.com" },
                Room = new Room { RoomName = "R101", Building = new Building { BuildingName = "Block A" } }
            };

            _mockManagerRepo.Setup(r => r.GetByAccountIdAsync(request.AccountId)).ReturnsAsync(manager);
            _mockAccountRepo.Setup(r => r.GetAccountByStudentId(request.StudentId)).ReturnsAsync(account);

            // QUAN TRỌNG: Mock trả về 3 lỗi (Đủ điều kiện đuổi)
            _mockViolationRepo.Setup(r => r.CountViolationsByStudentId(request.StudentId)).ReturnsAsync(3);

            _mockContractRepo.Setup(r => r.GetActiveContractByStudentId(request.StudentId)).ReturnsAsync(activeContract);
            _mockViolationRepo.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(new Violation());

            // Act
            var result = await _service.CreateViolationAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Contains("contract terminated", result.Message); // Kiểm tra message trả về

            // 1. Kiểm tra Contract có bị chuyển sang Terminated không
            Assert.Equal("Terminated", activeContract.ContractStatus);
            Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), activeContract.EndDate);
            _mockContractRepo.Verify(r => r.Update(activeContract), Times.Once);

            // 2. Kiểm tra có gửi Mail thông báo đuổi học không
            _mockEmailService.Verify(e => e.SendTerminatedNotiToStudentAsync(It.Is<DormTerminationDto>(
                dto => dto.StudentId == "S_BadBoy" && dto.ContractCode == "C1"
            )), Times.Once);

            // 3. Kiểm tra Commit (Ít nhất 2 lần: 1 lần tạo lỗi, 1 lần update hợp đồng)
            _mockUow.Verify(u => u.CommitAsync(), Times.AtLeast(2));
        }

        // ==========================================
        // UPDATE VIOLATION
        // ==========================================

        [Fact(DisplayName = "UpdateViolation: Thất bại (404) khi Vi phạm không tồn tại")]
        public async Task UpdateViolation_ShouldFail_WhenNotFound()
        {
            // Arrange
            _mockViolationRepo.Setup(r => r.GetByIdAsync("V_Ghost")).ReturnsAsync((Violation)null);

            // Act
            var result = await _service.UpdateViolationAsync(new UpdateViolationRequest { ViolationId = "V_Ghost" });

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact(DisplayName = "UpdateViolation: Thành công (Update DB & Bắn SignalR)")]
        public async Task UpdateViolation_ShouldSuccess_AndTriggerSignalR()
        {
            // Arrange
            var req = new UpdateViolationRequest { ViolationId = "V1", Resolution = "Phạt 500k" };

            var violation = new Violation { ViolationID = "V1", StudentID = "S1", ViolationAct = "Noise" };
            var account = new Account { UserId = "Acc_S1" }; // UserID để bắn SignalR

            _mockViolationRepo.Setup(r => r.GetByIdAsync(req.ViolationId)).ReturnsAsync(violation);
            _mockAccountRepo.Setup(r => r.GetAccountByStudentId("S1")).ReturnsAsync(account);

            // Mock SignalR
            _mockHubClients.Setup(c => c.User("Acc_S1")).Returns(_mockClientProxy.Object);

            // Act
            var result = await _service.UpdateViolationAsync(req);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);

            // Verify Logic DB
            Assert.Equal("Phạt 500k", violation.Resolution);
            _mockViolationRepo.Verify(r => r.Update(violation), Times.Once);
            _mockNotificationRepo.Verify(r => r.Add(It.IsAny<Notification>()), Times.Once);

            // Verify SignalR
            // Kiểm tra xem hàm SendCoreAsync có được gọi với method "ReceiveNotification" không
            _mockClientProxy.Verify(c => c.SendCoreAsync(
                "ReceiveNotification",
                It.Is<object[]>(args => args.Length > 0),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        // ==========================================
        // GET DATA & REPORT
        // ==========================================

        [Fact(DisplayName = "GetViolationsByStudentAccountId: Thất bại khi AccountID rỗng")]
        public async Task GetByAccId_ShouldFail_WhenEmpty()
        {
            var result = await _service.GetViolationsByStudentAccountIdAsync("");
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact(DisplayName = "GetAllViolationsByManager: Thành công (Map dữ liệu Student & Room)")]
        public async Task GetAllViolationsByManager_ShouldReturnListWithDetails()
        {
            // Arrange
            string accId = "Acc_Mgr";
            var manager = new BuildingManager { ManagerID = "M1" };

            var violations = new List<Violation>
            {
                new Violation { ViolationID = "V1", StudentID = "S1", ViolationAct = "Lỗi 1" },
                new Violation { ViolationID = "V2", StudentID = "S1", ViolationAct = "Lỗi 2" }
            };

            // Mock Contract để lấy thông tin phòng
            var contract = new Contract
            {
                ContractStatus = "Active",
                RoomID = "R101",
                Room = new Room { RoomName = "Phòng 101" }
            };

            _mockManagerRepo.Setup(r => r.GetByAccountIdAsync(accId)).ReturnsAsync(manager);
            _mockViolationRepo.Setup(r => r.GetByManagerId("M1")).ReturnsAsync(violations);

            // Mock trả về Contract cho SV S1
            _mockContractRepo.Setup(r => r.GetActiveContractByStudentId("S1")).ReturnsAsync(contract);

            // Act
            var result = await _service.GetAllViolationsByManagerAsync(accId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data.Count());

            var firstItem = result.Data.First();
            Assert.Equal("S1", firstItem.StudentId);
            Assert.Equal("R101", firstItem.RoomId);     // Map được từ Contract
            Assert.Equal("Phòng 101", firstItem.RoomName);
            Assert.Equal(2, firstItem.TotalViolationsOfStudent); // S1 có 2 lỗi
        }

        [Fact(DisplayName = "GetViolationStatsByManager: Thành công (Group theo sinh viên)")]
        public async Task GetViolationStatsByManager_ShouldGroupCorrectly()
        {
            // Arrange
            string accId = "Acc_Mgr";
            var manager = new BuildingManager { ManagerID = "M1" };

            // Giả lập danh sách lỗi: S1 bị 2 lần, S2 bị 1 lần
            var violations = new List<Violation>
            {
                new Violation { StudentID = "S1", Student = new Student { FullName = "Nam" } },
                new Violation { StudentID = "S1", Student = new Student { FullName = "Nam" } },
                new Violation { StudentID = "S2", Student = new Student { FullName = "Nu" } }
            };

            _mockManagerRepo.Setup(r => r.GetByAccountIdAsync(accId)).ReturnsAsync(manager);
            _mockViolationRepo.Setup(r => r.GetByManagerId("M1")).ReturnsAsync(violations);
            _mockContractRepo.Setup(r => r.GetActiveContractByStudentId(It.IsAny<string>())).ReturnsAsync(new Contract());

            // Act
            var result = await _service.GetViolationStatsByManagerAsync(accId);

            // Assert
            Assert.True(result.Success);
            var list = result.Data.ToList();

            Assert.Equal(2, list.Count); // Có 2 sinh viên trong danh sách thống kê

            var s1Stats = list.FirstOrDefault(x => x.StudentId == "S1");
            Assert.Equal(2, s1Stats.TotalViolations);

            var s2Stats = list.FirstOrDefault(x => x.StudentId == "S2");
            Assert.Equal(1, s2Stats.TotalViolations);
        }

        [Fact(DisplayName = "GetPendingViolations: Trả về danh sách chờ xử lý")]
        public async Task GetPendingViolations_ShouldReturnList()
        {
            // Arrange
            var violations = new List<Violation>
            {
                new Violation { ViolationID = "V1", StudentID = "S1" }
            };
            _mockViolationRepo.Setup(r => r.GetPendingViolations()).ReturnsAsync(violations);

            // Act
            var result = await _service.GetPendingViolationsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data);
            Assert.Equal("V1", result.Data.First().ViolationId);
        }
    }
}