using API.Services.Implements;
using API.UnitOfWorks;
using BusinessObject.DTOs.StudentDTOs;
using BusinessObject.Entities;
using DataAccess.Interfaces;
using DocumentFormat.OpenXml.ExtendedProperties;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Services.Implements
{
    public class StudentServiceTests
    {
        private readonly Mock<IStudentUow> _mockUow;
        private readonly Mock<IStudentRepository> _mockStudentRepo;
        private readonly Mock<IRelativeRepository> _mockRelativeRepo;
        private readonly Mock<IContractRepository> _mockContractRepo;
        private readonly Mock<IUtilityBillRepository> _mockUtilityBillRepo;
        private readonly Mock<IViolationRepository> _mockViolationRepo;
        private readonly Mock<IHealthInsuranceRepository> _mockInsuranceRepo;
        private readonly StudentService _service;

        public StudentServiceTests()
        {
            // 1. Khởi tạo các Mock Repository
            _mockUow = new Mock<IStudentUow>();
            _mockStudentRepo = new Mock<IStudentRepository>();
            _mockRelativeRepo = new Mock<IRelativeRepository>();
            _mockContractRepo = new Mock<IContractRepository>();
            _mockUtilityBillRepo = new Mock<IUtilityBillRepository>();
            _mockViolationRepo = new Mock<IViolationRepository>();
            _mockInsuranceRepo = new Mock<IHealthInsuranceRepository>();

            // 2. Setup UoW trả về các Repository tương ứng
            _mockUow.Setup(u => u.Students).Returns(_mockStudentRepo.Object);
            _mockUow.Setup(u => u.Relatives).Returns(_mockRelativeRepo.Object);
            _mockUow.Setup(u => u.Contracts).Returns(_mockContractRepo.Object);
            _mockUow.Setup(u => u.UtilityBills).Returns(_mockUtilityBillRepo.Object);
            _mockUow.Setup(u => u.Violations).Returns(_mockViolationRepo.Object);
            _mockUow.Setup(u => u.HealthInsurances).Returns(_mockInsuranceRepo.Object);

            // 3. Setup Transaction
            _mockUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

            // 4. Khởi tạo Service cần test
            _service = new StudentService(_mockUow.Object);
        }

        #region GetStudentByID

        [Fact(DisplayName = "GetStudentByID: Trả về lỗi 400 nếu AccountID bị rỗng")]
        public async Task GetStudentByID_EmptyId_ReturnsBadRequest()
        {
            // Act
            var result = await _service.GetStudentByID("");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Student ID is required.", result.Message);
        }

        [Fact(DisplayName = "GetStudentByID: Trả về lỗi 404 nếu không tìm thấy sinh viên")]
        public async Task GetStudentByID_NotFound_ReturnsNotFound()
        {
            // Arrange
            _mockStudentRepo.Setup(r => r.GetStudentByAccountIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Student)null);

            // Act
            var result = await _service.GetStudentByID("acc123");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact(DisplayName = "GetStudentByID: Trả về thông tin sinh viên và người thân nếu tìm thấy")]
        public async Task GetStudentByID_Found_ReturnsData()
        {
            // Arrange
            var student = new Student
            {
                StudentID = "S001",
                FullName = "Nguyen Van A",
                School = new School { SchoolName = "FPT" },
                Priority = new Priority { PriorityDescription = "Normal" },
                Relatives = new List<Relative>
                {
                    new Relative { FullName = "Mom", Relationship = "Mother" }
                }
            };

            _mockStudentRepo.Setup(r => r.GetStudentByAccountIdAsync("acc123"))
                .ReturnsAsync(student);

            // Act
            var result = await _service.GetStudentByID("acc123");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.student);
            Assert.Equal("Nguyen Van A", result.student.FullName);
            Assert.Single(result.student.Relatives);
        }

        #endregion

        #region UpdateStudent

        [Fact(DisplayName = "UpdateStudent: Cập nhật thành công và Commit Transaction")]
        public async Task UpdateStudent_Success_CommitsTransaction()
        {
            // Arrange
            var dto = new StudentUpdateInfoDTO { StudentID = "S001", FullName = "New Name" };
            var existingStudent = new Student { StudentID = "S001", FullName = "Old Name" };

            _mockStudentRepo.Setup(r => r.GetByIdAsync(dto.StudentID)).ReturnsAsync(existingStudent);

            // Act
            var result = await _service.UpdateStudent(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("New Name", existingStudent.FullName); // Kiểm tra object đã được cập nhật giá trị mới chưa

            // Kiểm tra Transaction có chạy đúng quy trình không
            _mockUow.Verify(u => u.BeginTransactionAsync(), Times.Once);
            _mockStudentRepo.Verify(r => r.Update(existingStudent), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "UpdateStudent: Rollback Transaction khi xảy ra lỗi DB")]
        public async Task UpdateStudent_Exception_RollbacksTransaction()
        {
            // Arrange
            var dto = new StudentUpdateInfoDTO { StudentID = "S001" };
            var existingStudent = new Student { StudentID = "S001" };

            _mockStudentRepo.Setup(r => r.GetByIdAsync(dto.StudentID)).ReturnsAsync(existingStudent);

            // Giả lập lỗi khi commit
            _mockUow.Setup(u => u.CommitAsync()).ThrowsAsync(new Exception("DB Error"));

            // Act
            var result = await _service.UpdateStudent(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);

            // Kiểm tra xem có gọi Rollback không
            _mockUow.Verify(u => u.RollbackAsync(), Times.Once);
        }

        #endregion

        #region CreateRelativesForStudent

        [Fact(DisplayName = "CreateRelatives: Tạo người thân mới thành công")]
        public async Task CreateRelative_Success_AddsEntity()
        {
            // Arrange
            var dto = new CreateRelativeDTO
            {
                StudentID = "S001",
                FullName = "Dad",
                Relationship = "Father"
            };

            _mockStudentRepo.Setup(r => r.GetByIdAsync("S001")).ReturnsAsync(new Student());

            // Act
            var result = await _service.CreateRelativesForStudent(dto);

            // Assert
            Assert.True(result.Success);
            // Kiểm tra xem hàm Add có được gọi với đúng dữ liệu không
            _mockRelativeRepo.Verify(r => r.Add(It.Is<Relative>(rel =>
                rel.FullName == "Dad" &&
                rel.Relationship == "Father" &&
                rel.RelativeID.StartsWith("REL-")
            )), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "CreateRelatives: Trả về lỗi 404 nếu StudentID không tồn tại")]
        public async Task CreateRelative_StudentNotFound_Returns404()
        {
            // Arrange
            _mockStudentRepo.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((Student)null);
            var dto = new CreateRelativeDTO { StudentID = "S999" };

            // Act
            var result = await _service.CreateRelativesForStudent(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
        }

        #endregion

        #region DeleteRelative

        [Fact(DisplayName = "DeleteRelative: Xóa người thân thành công")]
        public async Task DeleteRelative_Success_DeletesEntity()
        {
            // Arrange
            var relative = new Relative { RelativeID = "REL-001" };
            _mockRelativeRepo.Setup(r => r.GetByIdAsync("REL-001")).ReturnsAsync(relative);

            // Act
            var result = await _service.DeleteRelative("REL-001");

            // Assert
            Assert.True(result.Success);
            _mockRelativeRepo.Verify(r => r.Delete(relative), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        #endregion

        #region GetDashboardByStudentId

        [Fact(DisplayName = "GetDashboard: Trả về dữ liệu tổng hợp (Hợp đồng, Hóa đơn, Vi phạm)")]
        public async Task GetDashboard_Success_ReturnsAggregatedData()
        {
            // Arrange
            string accId = "acc1";
            string studentId = "S001";
            string roomId = "R101";

            var student = new Student { StudentID = studentId };

            // Mock Contract phức tạp
            var contract = new Contract
            {
                ContractID = "C001",
                StartDate = new DateOnly(2025, 1, 1),
                ContractStatus = "Active",
                RoomID = roomId,
                Room = new Room
                {
                    RoomName = "Room 101",
                    RoomType = new RoomType { TypeName = "Standard", Price = 100 },
                    Building = new Building
                    {
                        BuildingName = "Block A",
                        Manager = new BuildingManager
                        {
                            FullName = "Mr. Manager",
                            Email = "mgr@test.com",
                            PhoneNumber = "0999"
                        }
                    }
                }
            };

            var insurance = new HealthInsurance
            {
                EndDate = new DateOnly(2025, 12, 31)
            };

            // Setup calls
            _mockStudentRepo.Setup(r => r.GetStudentByAccountIdAsync(accId)).ReturnsAsync(student);
            _mockContractRepo.Setup(r => r.GetLastContractByStudentIdAsync(studentId)).ReturnsAsync(contract);
            _mockUtilityBillRepo.Setup(r => r.CountUnpaidBillByRoomAsync(roomId)).ReturnsAsync(2);
            _mockViolationRepo.Setup(r => r.CountViolationsByStudentId(studentId)).ReturnsAsync(1);
            _mockInsuranceRepo.Setup(r => r.GetActiveInsuranceByStudentIdAsync(studentId)).ReturnsAsync(insurance);

            // Act
            var result = await _service.GetDashboardByStudentId(accId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.dto);

            // Kiểm tra mapping
            Assert.Equal("C001", result.dto.CurrentContract.ContractID);
            Assert.Equal("Mr. Manager", result.dto.CurrentContract.ManagerName);

            // Kiểm tra số liệu
            Assert.Equal(2, result.dto.CountUnpaidBills);
            Assert.Equal(1, result.dto.CountViolations);
            Assert.Equal("Active", result.dto.InsuranceStatus);
        }

        [Fact(DisplayName = "GetDashboard: Trả về lỗi 404 nếu sinh viên chưa có hợp đồng")]
        public async Task GetDashboard_NoContract_ReturnsNotFound()
        {
            // Arrange
            var student = new Student { StudentID = "S001" };
            _mockStudentRepo.Setup(r => r.GetStudentByAccountIdAsync("acc1")).ReturnsAsync(student);
            _mockContractRepo.Setup(r => r.GetLastContractByStudentIdAsync("S001")).ReturnsAsync((Contract)null);

            // Act
            var result = await _service.GetDashboardByStudentId("acc1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("No contract found for the student.", result.Message);
            Assert.Equal(404, result.StatusCode);
        }

        #endregion
    }
}