using API.Services.Implements;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.ConfirmDTOs;
using BusinessObject.DTOs.RegisDTOs;
using BusinessObject.Entities;
using DataAccess.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Services.Implements
{
    public class RegisServiceTests
    {
        private readonly Mock<IRegistrationUow> _mockUow;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ILogger<IRegistrationService>> _mockLogger;

        
        private readonly Mock<IStudentRepository> _mockStudentRepo;
        private readonly Mock<IContractRepository> _mockContractRepo;
        private readonly Mock<IRoomRepository> _mockRoomRepo;
        private readonly Mock<IRegistrationFormRepository> _mockFormRepo;
        private readonly Mock<IReceiptRepository> _mockReceiptRepo;

        private readonly RegistrationService _service;

        public RegisServiceTests()
        {
            // 2. Khởi tạo Mock
            _mockUow = new Mock<IRegistrationUow>();
            _mockEmailService = new Mock<IEmailService>();
            _mockLogger = new Mock<ILogger<IRegistrationService>>();

            _mockStudentRepo = new Mock<IStudentRepository>();
            _mockContractRepo = new Mock<IContractRepository>();
            _mockRoomRepo = new Mock<IRoomRepository>();
            _mockFormRepo = new Mock<IRegistrationFormRepository>();
            _mockReceiptRepo = new Mock<IReceiptRepository>();

            // 3. Setup UoW trả về các Repo giả
            _mockUow.Setup(u => u.Students).Returns(_mockStudentRepo.Object);
            _mockUow.Setup(u => u.Contracts).Returns(_mockContractRepo.Object);
            _mockUow.Setup(u => u.Rooms).Returns(_mockRoomRepo.Object);
            _mockUow.Setup(u => u.RegistrationForms).Returns(_mockFormRepo.Object);
            _mockUow.Setup(u => u.Receipts).Returns(_mockReceiptRepo.Object);

            // Setup Transaction giả
            _mockUow.Setup(u => u.BeginTransactionAsync(It.IsAny<System.Data.IsolationLevel>())).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

            // 4. Inject vào Service
            _service = new RegistrationService(_mockUow.Object, _mockEmailService.Object, _mockLogger.Object);
        }

        // ==========================================================
        // PHẦN 1: TEST CREATE REGISTRATION FORM
        // ==========================================================

        [Fact]
        public async Task CreateRegistrationForm_ShouldReturnSuccess_WhenValid()
        {
            // Arrange
            var request = new RegistrationFormRequest { AccountId = "Acc1", RoomId = "Room1" };
            var student = new Student { StudentID = "S1", Gender = "Male" };
            var room = new Room { RoomID = "Room1", Capacity = 4, Gender = "Male" };

            // Setup: Có student, chưa có contract active
            _mockStudentRepo.Setup(r => r.GetStudentByAccountIdAsync(request.AccountId)).ReturnsAsync(student);
            _mockContractRepo.Setup(r => r.GetActiveContractByStudentId("S1")).ReturnsAsync((Contract)null);

            // Setup: Phòng còn chỗ (1 người đang ở + 1 đơn pending = 2 < 4)
            _mockContractRepo.Setup(r => r.CountContractsByRoomIdAndStatus(request.RoomId, "Active")).ReturnsAsync(1);
            _mockFormRepo.Setup(r => r.CountRegistrationFormsByRoomId(request.RoomId)).ReturnsAsync(1);
            _mockRoomRepo.Setup(r => r.GetByIdAsync(request.RoomId)).ReturnsAsync(room);

            // Act
            var result = await _service.CreateRegistrationForm(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);
            Assert.NotNull(result.registrationId);

            _mockFormRepo.Verify(r => r.Add(It.IsAny<RegistrationForm>()), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateRegistrationForm_ShouldFail_WhenStudentHasActiveContract()
        {
            // Arrange
            var request = new RegistrationFormRequest { AccountId = "Acc1" };
            var student = new Student { StudentID = "S1" };
            var activeContract = new Contract { ContractID = "C1" };

            _mockStudentRepo.Setup(r => r.GetStudentByAccountIdAsync(request.AccountId)).ReturnsAsync(student);
            _mockContractRepo.Setup(r => r.GetActiveContractByStudentId("S1")).ReturnsAsync(activeContract);

            // Act
            var result = await _service.CreateRegistrationForm(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Student already has an active contract.", result.Message);
            _mockUow.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateRegistrationForm_ShouldFail_WhenGenderMismatch()
        {
            // Arrange
            var request = new RegistrationFormRequest { AccountId = "Acc1", RoomId = "Room1" };
            var student = new Student { StudentID = "S1", Gender = "Male" };
            var room = new Room { RoomID = "Room1", Gender = "Female", Capacity = 4 }; // Khác giới tính

            _mockStudentRepo.Setup(r => r.GetStudentByAccountIdAsync(request.AccountId)).ReturnsAsync(student);
            _mockContractRepo.Setup(r => r.GetActiveContractByStudentId("S1")).ReturnsAsync((Contract)null);
            _mockRoomRepo.Setup(r => r.GetByIdAsync(request.RoomId)).ReturnsAsync(room);

            // Act
            var result = await _service.CreateRegistrationForm(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Gender is not suitable", result.Message);
            _mockUow.Verify(u => u.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateRegistrationForm_ShouldFail_WhenRoomIsFull()
        {
            // Arrange
            var request = new RegistrationFormRequest { AccountId = "Acc1", RoomId = "Room1" };
            var student = new Student { StudentID = "S1", Gender = "Male" };
            var room = new Room { RoomID = "Room1", Capacity = 4, Gender = "Male" };

            _mockStudentRepo.Setup(r => r.GetStudentByAccountIdAsync(request.AccountId)).ReturnsAsync(student);
            _mockContractRepo.Setup(r => r.GetActiveContractByStudentId("S1")).ReturnsAsync((Contract)null);
            _mockRoomRepo.Setup(r => r.GetByIdAsync(request.RoomId)).ReturnsAsync(room);

            // Setup: Phòng full (2 người đang ở + 2 đơn pending = 4 >= 4)
            _mockContractRepo.Setup(r => r.CountContractsByRoomIdAndStatus(request.RoomId, "Active")).ReturnsAsync(2);
            _mockFormRepo.Setup(r => r.CountRegistrationFormsByRoomId(request.RoomId)).ReturnsAsync(2);

            // Act
            var result = await _service.CreateRegistrationForm(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(409, result.StatusCode);
            Assert.Equal("Room is already full.", result.Message);
            _mockUow.Verify(u => u.RollbackAsync(), Times.Once);
        }
        [Fact]
        public async Task ConfirmPayment_ShouldSuccess_WhenValid_AndCreateContract()
        {
            // Arrange
            string regisId = "RF-001";
            var regisForm = new RegistrationForm { FormID = "RF-001", StudentID = "S1", RoomID = "R1", Status = "Pending" };

            // Setup đối tượng Room lồng nhau phức tạp để tránh NullRef khi map EmailDto
            var roomType = new RoomType { TypeName = "Deluxe", Price = 500 };
            var building = new Building { BuildingName = "Block A" };
            var room = new Room
            {
                RoomID = "R1",
                RoomName = "101",
                RoomType = roomType,
                Building = building,
                CurrentOccupancy = 0,
                Capacity = 4
            };

            var student = new Student { StudentID = "S1", Email = "student@test.com", FullName = "Test Name" };
            var receipt = new Receipt { Amount = 1000, Status = "Pending" };

            // Mock Data Returns
            _mockFormRepo.Setup(r => r.GetByIdAsync(regisId)).ReturnsAsync(regisForm);
            _mockRoomRepo.Setup(r => r.GetByIdAsync("R1")).ReturnsAsync(room);
            _mockReceiptRepo.Setup(r => r.GetReceiptByTypeAndRelatedIdAsync("Registration", "RF-001")).ReturnsAsync(receipt);
            _mockStudentRepo.Setup(r => r.GetByIdAsync("S1")).ReturnsAsync(student);

            // Act
            var result = await _service.ConfirmPaymentForRegistration(regisId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);

            // 1. Verify Receipt updated
            Assert.Equal("Success", receipt.Status);
            _mockReceiptRepo.Verify(r => r.Update(receipt), Times.Once);

            // 2. Verify Room updated (Logic AddOccupant trong code bạn cần đảm bảo entity tăng số lượng)
            // Lưu ý: Test này check xem hàm Update có được gọi ko
            _mockRoomRepo.Verify(r => r.Update(room), Times.Once);

            // 3. Verify Contract created
            _mockContractRepo.Verify(r => r.Add(It.IsAny<Contract>()), Times.Once);

            // 4. Verify Registration Form status updated
            Assert.Equal("Confirmed", regisForm.Status);

            // 5. Verify Email Sent
            _mockEmailService.Verify(e => e.SendRegistrationPaymentEmailAsync(It.IsAny<DormRegistrationSuccessDto>()), Times.Once);

            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task ConfirmPayment_ShouldStillSuccess_EvenIfEmailFails()
        {
            // Test Case: DB Transaction thành công nhưng gửi mail bị lỗi -> Vẫn phải return True (vì tiền đã trừ)

            // Arrange (Setup tương tự case trên)
            string regisId = "RF-001";
            var regisForm = new RegistrationForm { FormID = "RF-001", StudentID = "S1", RoomID = "R1" };
            var room = new Room
            {
                RoomID = "R1",
                RoomType = new RoomType(),
                Building = new Building()
            };
            var student = new Student { StudentID = "S1" };
            var receipt = new Receipt { Amount = 1000 };

            _mockFormRepo.Setup(r => r.GetByIdAsync(regisId)).ReturnsAsync(regisForm);
            _mockRoomRepo.Setup(r => r.GetByIdAsync("R1")).ReturnsAsync(room);
            _mockReceiptRepo.Setup(r => r.GetReceiptByTypeAndRelatedIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(receipt);
            _mockStudentRepo.Setup(r => r.GetByIdAsync("S1")).ReturnsAsync(student);

            // Setup: Email Service ném Exception
            _mockEmailService.Setup(e => e.SendRegistrationPaymentEmailAsync(It.IsAny<DormRegistrationSuccessDto>()))
                             .ThrowsAsync(new Exception("SMTP Error"));

            // Act
            var result = await _service.ConfirmPaymentForRegistration(regisId);

            // Assert
            Assert.True(result.Success); // Vẫn phải true
            Assert.Equal(200, result.StatusCode);

            // Verify: Commit vẫn phải chạy dù mail lỗi
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);

            // Verify: Logger phải log lỗi
            // Cách verify Logger extension method
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task ConfirmPayment_ShouldFail_WhenReceiptNotFound()
        {
            // Arrange
            string regisId = "RF-001";
            var regisForm = new RegistrationForm { FormID = "RF-001", RoomID = "R1" };
            var room = new Room { RoomID = "R1" };

            _mockFormRepo.Setup(r => r.GetByIdAsync(regisId)).ReturnsAsync(regisForm);
            _mockRoomRepo.Setup(r => r.GetByIdAsync("R1")).ReturnsAsync(room);

            // Setup: Không tìm thấy receipt
            _mockReceiptRepo.Setup(r => r.GetReceiptByTypeAndRelatedIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                            .ReturnsAsync((Receipt)null);

            // Act
            var result = await _service.ConfirmPaymentForRegistration(regisId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Associated receipt not found.", result.Message);
            _mockUow.Verify(u => u.CommitAsync(), Times.Never);
        }
    }
}
