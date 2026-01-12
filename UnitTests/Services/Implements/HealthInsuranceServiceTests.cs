using API.Services.Implements;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.ConfirmDTOs;
using BusinessObject.DTOs.HealthInsuranceDTOs;
using BusinessObject.Entities;
using DataAccess.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;


namespace UnitTests.Services.Implements
{
    public class HealthInsuranceServiceTests
    {
        // 1. Khai báo Mock
        private readonly Mock<IHealthInsuranceUow> _mockUow;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ILogger<HealthInsuranceService>> _mockLogger;
        private readonly HealthInsuranceService _service;

        // Mock các Repository con
        private readonly Mock<IStudentRepository> _mockStudentRepo;
        private readonly Mock<IHealthInsuranceRepository> _mockHealthInsuranceRepo;
        private readonly Mock<IHealthPriceRepository> _mockHealthPriceRepo;
        private readonly Mock<INotificationRepository> _mockNotificationRepo;

        public HealthInsuranceServiceTests()
        {
            // 2. Khởi tạo Mock
            _mockUow = new Mock<IHealthInsuranceUow>();
            _mockEmailService = new Mock<IEmailService>();
            _mockLogger = new Mock<ILogger<HealthInsuranceService>>();

            _mockStudentRepo = new Mock<IStudentRepository>();
            _mockHealthInsuranceRepo = new Mock<IHealthInsuranceRepository>();
            _mockHealthPriceRepo = new Mock<IHealthPriceRepository>();
            _mockNotificationRepo = new Mock<INotificationRepository>();

            // 3. Setup UOW trả về các Repo con
            _mockUow.Setup(u => u.Students).Returns(_mockStudentRepo.Object);
            _mockUow.Setup(u => u.HealthInsurances).Returns(_mockHealthInsuranceRepo.Object);
            _mockUow.Setup(u => u.HealthPrices).Returns(_mockHealthPriceRepo.Object);
            _mockUow.Setup(u => u.Notifications).Returns(_mockNotificationRepo.Object);

            // Mock Transaction
            _mockUow.Setup(u => u.BeginTransactionAsync(It.IsAny<System.Data.IsolationLevel>())).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

            // 4. Inject Service
            _service = new HealthInsuranceService(_mockUow.Object, _mockEmailService.Object, _mockLogger.Object);
        }

        // =========================================================================
        // REGISTER HEALTH INSURANCE
        // =========================================================================

        [Fact(DisplayName = "Register: Thất bại khi không tìm thấy sinh viên (404)")]
        public async Task Register_ShouldFail_WhenStudentNotFound()
        {
            // Arrange
            string studentId = "S_Ghost";
            _mockStudentRepo.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync((Student)null);

            // Act
            var result = await _service.RegisterHealthInsuranceAsync(studentId, "Hosp1", "Card123");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Student not found.", result.Message);
        }

        [Fact(DisplayName = "Register: Thất bại khi đã có yêu cầu đang chờ duyệt (Pending)")]
        public async Task Register_ShouldFail_WhenPendingRequestExists()
        {
            // Arrange
            string studentId = "S1";
            _mockStudentRepo.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync(new Student());

            // Setup: Đã có đơn pending
            _mockHealthInsuranceRepo.Setup(r => r.HasPendingInsuranceRequestAsync(studentId)).ReturnsAsync(true);

            // Act
            var result = await _service.RegisterHealthInsuranceAsync(studentId, "Hosp1", "Card123");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(409, result.StatusCode);
            Assert.Contains("already have a pending", result.Message);
        }

        [Fact(DisplayName = "Register: Thất bại khi bảo hiểm hiện tại còn hạn trên 1 tháng")]
        public async Task Register_ShouldFail_WhenCurrentInsuranceStillValidForMoreThan1Month()
        {
            // Arrange
            string studentId = "S1";
            _mockStudentRepo.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync(new Student());
            _mockHealthInsuranceRepo.Setup(r => r.HasPendingInsuranceRequestAsync(studentId)).ReturnsAsync(false);

            // Giả lập bảo hiểm còn hạn 2 tháng nữa mới hết
            var activeInsurance = new HealthInsurance { EndDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(2)) };
            _mockHealthInsuranceRepo.Setup(r => r.GetActiveInsuranceByStudentIdAsync(studentId)).ReturnsAsync(activeInsurance);

            // Act
            var result = await _service.RegisterHealthInsuranceAsync(studentId, "Hosp1", "Card123");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Renewal is only allowed 1 months before expiration", result.Message);
        }

        [Fact(DisplayName = "Register: Thất bại khi không tìm thấy bảng giá bảo hiểm cho năm sau")]
        public async Task Register_ShouldFail_WhenPriceNotFound()
        {
            // Arrange
            string studentId = "S1";
            int nextYear = DateTime.Now.Year + 1;

            _mockStudentRepo.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync(new Student());
            _mockHealthInsuranceRepo.Setup(r => r.HasPendingInsuranceRequestAsync(studentId)).ReturnsAsync(false);
            _mockHealthInsuranceRepo.Setup(r => r.GetActiveInsuranceByStudentIdAsync(studentId)).ReturnsAsync((HealthInsurance)null);

            // Setup: Giá null
            _mockHealthPriceRepo.Setup(r => r.GetHealthInsuranceByYear(nextYear)).ReturnsAsync((HealthInsurancePrice)null);

            // Act
            var result = await _service.RegisterHealthInsuranceAsync(studentId, "Hosp1", "Card123");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Không thể lấy giá bảo hiểm", result.Message);
        }

        [Fact(DisplayName = "Register: Thành công (Tạo đơn mới & Transaction Commit)")]
        public async Task Register_ShouldSuccess_WhenValidConditions()
        {
            // Arrange
            string studentId = "S1";
            string hospitalId = "H1";
            int nextYear = DateTime.Now.Year + 1;

            _mockStudentRepo.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync(new Student());
            _mockHealthInsuranceRepo.Setup(r => r.HasPendingInsuranceRequestAsync(studentId)).ReturnsAsync(false);
            _mockHealthInsuranceRepo.Setup(r => r.GetActiveInsuranceByStudentIdAsync(studentId)).ReturnsAsync((HealthInsurance)null);

            var price = new HealthInsurancePrice { HealthPriceID = "HP1", Amount = 500000 };
            _mockHealthPriceRepo.Setup(r => r.GetHealthInsuranceByYear(nextYear)).ReturnsAsync(price);

            // Act
            var result = await _service.RegisterHealthInsuranceAsync(studentId, hospitalId, "Card123");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);
            Assert.NotNull(result.insuranceId);

            // Verify Transaction
            _mockUow.Verify(u => u.BeginTransactionAsync(), Times.Once);

            // Verify Add đúng dữ liệu
            _mockHealthInsuranceRepo.Verify(r => r.Add(It.Is<HealthInsurance>(h =>
                h.StudentID == studentId &&
                h.HospitalID == hospitalId &&
                h.Status == "Pending" &&
                h.Cost == 500000 &&
                h.HealthPriceID == "HP1"
            )), Times.Once);

            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        // =========================================================================
        // CONFIRM INSURANCE PAYMENT
        // =========================================================================

        [Fact(DisplayName = "ConfirmPayment: Thất bại khi không tìm thấy mã bảo hiểm")]
        public async Task Confirm_ShouldFail_WhenInsuranceNotFound()
        {
            // Arrange
            string insuranceId = "Ins_Ghost";
            _mockHealthInsuranceRepo.Setup(r => r.GetDetailInsuranceByIdAsync(insuranceId)).ReturnsAsync((HealthInsurance)null);

            // Act
            var result = await _service.ConfirmInsurancePaymentAsync(insuranceId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact(DisplayName = "ConfirmPayment: Trả về thành công ngay nếu bảo hiểm đã Active trước đó")]
        public async Task Confirm_ShouldReturnTrue_WhenAlreadyActive()
        {
            // Arrange
            var insurance = new HealthInsurance { Status = "Active" };
            _mockHealthInsuranceRepo.Setup(r => r.GetDetailInsuranceByIdAsync("Ins1")).ReturnsAsync(insurance);

            // Act
            var result = await _service.ConfirmInsurancePaymentAsync("Ins1");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Insurance is already active.", result.Message);

            // Verify không gọi Update hay Gửi mail lại
            _mockHealthInsuranceRepo.Verify(r => r.Update(It.IsAny<HealthInsurance>()), Times.Never);
        }

        [Fact(DisplayName = "ConfirmPayment: Thành công (Kích hoạt bảo hiểm, Lưu thông báo & Gửi Email)")]
        public async Task Confirm_ShouldSuccess_UpdateStatusAndSendEmail()
        {
            // Arrange
            string insuranceId = "Ins1";
            var student = new Student
            {
                FullName = "Nguyen Van A",
                Email = "a@test.com",
                Account = new Account { UserId = "Acc1" }
            };

            var insurance = new HealthInsurance
            {
                InsuranceID = insuranceId,
                Status = "Pending",
                Student = student,
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 12, 31),
                Cost = 600000
            };

            _mockHealthInsuranceRepo.Setup(r => r.GetDetailInsuranceByIdAsync(insuranceId)).ReturnsAsync(insurance);

            // Act
            var result = await _service.ConfirmInsurancePaymentAsync(insuranceId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Active", insurance.Status); // Kiểm tra object đã đổi trạng thái

            // Verify DB updates
            _mockNotificationRepo.Verify(n => n.Add(It.Is<Notification>(noti => noti.AccountID == "Acc1")), Times.Once);
            _mockHealthInsuranceRepo.Verify(r => r.Update(insurance), Times.Once);

            // Verify Email sent
            _mockEmailService.Verify(e => e.SendInsurancePaymentEmailAsync(It.Is<HealthInsurancePurchaseDto>(
                dto => dto.StudentEmail == "a@test.com" && dto.Cost == 600000
            )), Times.Once);
        }

        // =========================================================================
        // CREATE HEALTH INSURANCE PRICE
        // =========================================================================

        [Fact(DisplayName = "CreatePrice: Thất bại khi số tiền bị âm")]
        public async Task CreatePrice_ShouldFail_WhenAmountIsNegative()
        {
            // Arrange
            var request = new CreateHealthPriceDTO { Amount = -100, Year = 2025 };

            // Act
            var result = await _service.CreateHealthInsurancePriceAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("không được âm", result.Message);
        }

        [Fact(DisplayName = "CreatePrice: Thành công (Hủy kích hoạt giá cũ & Tạo giá mới)")]
        public async Task CreatePrice_ShouldSuccess_AndDeactivateOldPrice()
        {
            // Arrange
            var request = new CreateHealthPriceDTO
            {
                Amount = 1000000,
                Year = 2026,
                EffectiveDate = DateOnly.FromDateTime(DateTime.Now)
            };

            var oldPrice = new HealthInsurancePrice { IsActive = true, Year = 2026 };
            _mockHealthPriceRepo.Setup(r => r.GetHealthInsuranceByYear(2026)).ReturnsAsync(oldPrice);

            // Act
            var result = await _service.CreateHealthInsurancePriceAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);

            // Verify giá cũ bị hủy kích hoạt
            Assert.False(oldPrice.IsActive);

            // Verify giá mới được thêm vào
            _mockHealthPriceRepo.Verify(r => r.Add(It.Is<HealthInsurancePrice>(
                p => p.Amount == 1000000 && p.IsActive == true && p.Year == 2026
            )), Times.Once);

            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }
    }
}