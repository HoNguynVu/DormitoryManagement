using API.Services.Implements;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.MaintenanceDTOs;
using BusinessObject.Entities;
using DataAccess.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Services.Implements
{
    public class MaintenanceServiceTests
    {
        // 1. Khai báo Mock
        private readonly Mock<IMaintenanceUow> _mockUow;
        private readonly Mock<IRoomEquipmentService> _mockRoomEquipmentService;
        private readonly MaintenanceService _service;

        // Mock Repositories con
        private readonly Mock<IContractRepository> _mockContractRepo;
        private readonly Mock<IMaintenanceRepository> _mockMaintenanceRepo;
        private readonly Mock<IReceiptRepository> _mockReceiptRepo;
        private readonly Mock<INotificationRepository> _mockNotificationRepo;

        public MaintenanceServiceTests()
        {
            // 2. Khởi tạo Mock
            _mockUow = new Mock<IMaintenanceUow>();
            _mockRoomEquipmentService = new Mock<IRoomEquipmentService>();

            _mockContractRepo = new Mock<IContractRepository>();
            _mockMaintenanceRepo = new Mock<IMaintenanceRepository>();
            _mockReceiptRepo = new Mock<IReceiptRepository>();
            _mockNotificationRepo = new Mock<INotificationRepository>();

            // 3. Setup UOW behavior
            _mockUow.Setup(u => u.Contracts).Returns(_mockContractRepo.Object);
            _mockUow.Setup(u => u.Maintenances).Returns(_mockMaintenanceRepo.Object);
            _mockUow.Setup(u => u.Receipts).Returns(_mockReceiptRepo.Object);
            _mockUow.Setup(u => u.Notifications).Returns(_mockNotificationRepo.Object);

            // Mock Transaction
            _mockUow.Setup(u => u.BeginTransactionAsync(It.IsAny<System.Data.IsolationLevel>())).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

            // 4. Inject Service
            _service = new MaintenanceService(_mockUow.Object, _mockRoomEquipmentService.Object);
        }

        // =========================================================
        // CREATE REQUEST
        // =========================================================

        [Fact(DisplayName = "CreateRequest: Thất bại khi thiếu thông tin đầu vào")]
        public async Task CreateRequest_ShouldFail_WhenInputInvalid()
        {
            // Arrange
            var dto = new CreateMaintenanceDto { StudentId = "", Description = "" };

            // Act
            var result = await _service.CreateRequestAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact(DisplayName = "CreateRequest: Thất bại khi sinh viên không có hợp đồng Active")]
        public async Task CreateRequest_ShouldFail_WhenNoActiveContract()
        {
            // Arrange
            var dto = new CreateMaintenanceDto { StudentId = "S1", Description = "Hỏng đèn" };

            // Mock trả về null (không có hợp đồng)
            _mockContractRepo.Setup(r => r.GetActiveAndNearExpiringContractByStudentId("S1")).ReturnsAsync((Contract)null);

            // Act
            var result = await _service.CreateRequestAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(403, result.StatusCode);
            Assert.Contains("chưa có hợp đồng", result.Message);
        }

        [Fact(DisplayName = "CreateRequest: Thành công (Tạo Request & Update trạng thái thiết bị)")]
        public async Task CreateRequest_ShouldSuccess_WhenValid()
        {
            // Arrange
            var dto = new CreateMaintenanceDto
            {
                StudentId = "S1",
                Description = "Hỏng đèn",
                EquipmentId = "EQ1"
            };

            var contract = new Contract
            {
                RoomID = "R1",
                Room = new Room { RoomID = "R1" }
            };

            _mockContractRepo.Setup(r => r.GetActiveAndNearExpiringContractByStudentId("S1")).ReturnsAsync(contract);
            _mockRoomEquipmentService.Setup(s => s.ChangeStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                                     .ReturnsAsync((true, "Ok", 200));

            // Act
            var result = await _service.CreateRequestAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);
            Assert.NotNull(result.requestMaintenanceId);

            // Verify gọi service đổi trạng thái thiết bị
            _mockRoomEquipmentService.Verify(s => s.ChangeStatusAsync("R1", "EQ1", 1, "Good", "Under Maintenance"), Times.Once);

            // Verify lưu DB
            _mockMaintenanceRepo.Verify(r => r.Add(It.IsAny<MaintenanceRequest>()), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        // =========================================================
        // UPDATE STATUS
        // =========================================================

        [Fact(DisplayName = "UpdateStatus: Thất bại khi không tìm thấy Request")]
        public async Task UpdateStatus_ShouldFail_WhenRequestNotFound()
        {
            // Arrange
            var dto = new UpdateMaintenanceStatusDto { RequestId = "MT-Ghost", NewStatus = "Completed" };
            _mockMaintenanceRepo.Setup(r => r.GetMaintenanceByIdAsync(dto.RequestId)).ReturnsAsync((MaintenanceRequest)null);

            // Act
            var result = await _service.UpdateStatusAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact(DisplayName = "UpdateStatus: Thành công (Chuyển In Progress -> Cập nhật thiết bị Under Maintenance)")]
        public async Task UpdateStatus_ShouldSuccess_InProgress()
        {
            // Arrange
            var dto = new UpdateMaintenanceStatusDto { RequestId = "MT1", NewStatus = "In Progress" };
            var request = new MaintenanceRequest { RequestID = "MT1", RoomID = "R1", EquipmentID = "EQ1" };

            _mockMaintenanceRepo.Setup(r => r.GetMaintenanceByIdAsync("MT1")).ReturnsAsync(request);

            // Act
            var result = await _service.UpdateStatusAsync(dto);

            // Assert
            Assert.True(result.Success);
            _mockRoomEquipmentService.Verify(s => s.ChangeStatusAsync("R1", "EQ1", 1, "Under Maintenance", "Being Repaired"), Times.Once);
            _mockMaintenanceRepo.Verify(r => r.Update(request), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "UpdateStatus: Thành công (Wait Payment -> Tạo hóa đơn)")]
        public async Task UpdateStatus_ShouldSuccess_WaitPayment_CreateReceipt()
        {
            // Arrange
            var dto = new UpdateMaintenanceStatusDto
            {
                RequestId = "MT1",
                NewStatus = "Wait Payment",
                RepairCost = 50000
            };
            var request = new MaintenanceRequest
            {
                RequestID = "MT1",
                StudentID = "S1",
                Description = "Fix Fan"
            };

            _mockMaintenanceRepo.Setup(r => r.GetMaintenanceByIdAsync("MT1")).ReturnsAsync(request);

            // Act
            var result = await _service.UpdateStatusAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(dto.RepairCost, request.RepairCost);

            // Verify Receipt Creation
            _mockReceiptRepo.Verify(r => r.Add(It.Is<Receipt>(x =>
                x.StudentID == "S1" &&
                x.Amount == 50000 &&
                x.RelatedObjectID == "MT1" &&
                x.Status == "Pending"
            )), Times.Once);

            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "UpdateStatus: Thành công (Completed -> Cập nhật thiết bị Good)")]
        public async Task UpdateStatus_ShouldSuccess_Completed()
        {
            // Arrange
            var dto = new UpdateMaintenanceStatusDto { RequestId = "MT1", NewStatus = "Completed" };
            var request = new MaintenanceRequest { RequestID = "MT1", RoomID = "R1", EquipmentID = "EQ1" };

            _mockMaintenanceRepo.Setup(r => r.GetMaintenanceByIdAsync("MT1")).ReturnsAsync(request);

            // Act
            var result = await _service.UpdateStatusAsync(dto);

            // Assert
            Assert.True(result.Success);
            _mockRoomEquipmentService.Verify(s => s.ChangeStatusAsync("R1", "EQ1", 1, "Being Repaired", "Good"), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        // =========================================================
        // CONFIRM PAYMENT
        // =========================================================

        [Fact(DisplayName = "ConfirmPayment: Thành công (Cập nhật Status, Tạo Notification)")]
        public async Task ConfirmPayment_ShouldSuccess()
        {
            // Arrange
            string mtId = "MT1";
            var student = new Student { FullName = "A", Account = new Account { UserId = "Acc1" } };
            var equipment = new Equipment { EquipmentName = "Fan" };
            var request = new MaintenanceRequest
            {
                RequestID = mtId,
                Status = "Wait Payment",
                Student = student,
                Equipment = equipment
            };

            _mockMaintenanceRepo.Setup(r => r.GetMaintenanceByIdAsync(mtId)).ReturnsAsync(request);

            // Act
            var result = await _service.ConfirmPaymentMaintenanceFee(mtId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Completed", request.Status);

            // Verify
            _mockNotificationRepo.Verify(n => n.Add(It.Is<Notification>(x => x.AccountID == "Acc1")), Times.Once);
            _mockMaintenanceRepo.Verify(r => r.Update(request), Times.Once);
        }

        // =========================================================
        // 4. GET REQUESTS BY STUDENT ID
        // =========================================================

        [Fact(DisplayName = "GetByStudentId: Trả về danh sách thành công")]
        public async Task GetByStudentId_ShouldReturnList_WhenFound()
        {
            // Arrange
            string studentId = "S1";
            var list = new List<MaintenanceRequest>
            {
                new MaintenanceRequest
                {
                    RequestID = "M1",
                    Room = new Room { RoomName = "R101" },
                    Student = new Student { FullName = "Nguyen Van A" },
                    Equipment = new Equipment { EquipmentName = "Den" },
                    RequestDate = DateTime.Now
                }
            };
            _mockMaintenanceRepo.Setup(r => r.GetMaintenanceByStudentIdAsync(studentId)).ReturnsAsync(list);

            // Act
            var result = await _service.GetRequestsByStudentIdAsync(studentId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Single(result.dto); // Kiểm tra có 1 phần tử
        }

        [Fact(DisplayName = "GetDetail: Trả về 404 nếu không thấy")]
        public async Task GetDetail_ShouldReturn404_WhenNotFound()
        {
            // Arrange
            _mockMaintenanceRepo.Setup(r => r.GetMaintenanceDetailAsync("MT_Ghost")).ReturnsAsync((MaintenanceRequest)null);

            // Act
            var result = await _service.GetMaintenanceDetail("MT_Ghost");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact(DisplayName = "GetOverview: Thành công trả về Dictionary")]
        public async Task GetOverview_ShouldReturnGroupedData()
        {
            // Arrange
            var list = new List<MaintenanceRequest>
            {
                new MaintenanceRequest { Status = "Pending" },
                new MaintenanceRequest { Status = "Pending" },
                new MaintenanceRequest { Status = "Completed" }
            };
            _mockMaintenanceRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(list);

            // Act
            var result = await _service.GetOverviewMaintenance();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.list["Pending"]);   // Có 2 cái pending
            Assert.Equal(1, result.list["Completed"]); // Có 1 cái completed
        }
    }
}