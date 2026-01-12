using API.Services.Implements;
using API.UnitOfWorks;
using BusinessObject.DTOs.BuildingManagerDTOs;
using BusinessObject.Entities;
using BusinessObject.Helpers;
using DataAccess.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Services.Implements
{ 
    public class BuildingManagerServiceTests
    {
        // 1. Main Mocks
        private readonly Mock<IBuildingUow> _mockUow;
        private readonly Mock<IBuildingManagerRepository> _mockManagerRepo;
        private readonly Mock<IBuildingRepository> _mockBuildingRepo;
        private readonly Mock<IAccountRepository> _mockAccountRepo;

        // 2. Mocks for Dashboard & Stats
        private readonly Mock<IRoomRepository> _mockRoomRepo;
        private readonly Mock<IUtilityBillRepository> _mockBillRepo;
        private readonly Mock<IStudentRepository> _mockStudentRepo;
        private readonly Mock<IMaintenanceRepository> _mockMaintenanceRepo;

        // 3. Mocks for Receipts
        private readonly Mock<IReceiptRepository> _mockReceiptRepo;

        // 4. Service under test
        private readonly BuildingManagerService _service;

        public BuildingManagerServiceTests()
        {
            _mockUow = new Mock<IBuildingUow>();
            _mockManagerRepo = new Mock<IBuildingManagerRepository>();
            _mockBuildingRepo = new Mock<IBuildingRepository>();
            _mockAccountRepo = new Mock<IAccountRepository>();
            _mockRoomRepo = new Mock<IRoomRepository>();
            _mockBillRepo = new Mock<IUtilityBillRepository>();
            _mockStudentRepo = new Mock<IStudentRepository>();
            _mockMaintenanceRepo = new Mock<IMaintenanceRepository>();
            _mockReceiptRepo = new Mock<IReceiptRepository>();

            // Setup UOW to return specific mocks
            _mockUow.Setup(u => u.BuildingManagers).Returns(_mockManagerRepo.Object);
            _mockUow.Setup(u => u.Buildings).Returns(_mockBuildingRepo.Object);
            _mockUow.Setup(u => u.Accounts).Returns(_mockAccountRepo.Object);
            _mockUow.Setup(u => u.Rooms).Returns(_mockRoomRepo.Object);
            _mockUow.Setup(u => u.UtilityBills).Returns(_mockBillRepo.Object);
            _mockUow.Setup(u => u.Students).Returns(_mockStudentRepo.Object);
            _mockUow.Setup(u => u.Maintenances).Returns(_mockMaintenanceRepo.Object);
            _mockUow.Setup(u => u.Receipts).Returns(_mockReceiptRepo.Object);

            // Transaction Setup
            _mockUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

            _service = new BuildingManagerService(_mockUow.Object);
        }

        #region Get Methods (GetAll, GetById, GetByAccount)

        [Fact]
        public async Task GetAllManagersAsync_ReturnsMappedDtos()
        {
            // Arrange
            var managers = new List<BuildingManager>
            {
                new BuildingManager
                {
                    ManagerID = "M1",
                    FullName = "Manager A",
                    Buildings = new List<Building> { new Building { BuildingID = "B1", BuildingName = "Block A" } }
                }
            };
            _mockManagerRepo.Setup(r => r.GetAllWithBuildingsAsync()).ReturnsAsync(managers);

            // Act
            var result = await _service.GetAllManagersAsync();

            // Assert
            Assert.NotEmpty(result);
            var first = result.First();
            Assert.Equal("M1", first.ManagerID);
            Assert.Equal("Block A", first.BuildingDto.BuildingName);
        }

        [Fact]
        public async Task GetManagerByIdAsync_ReturnsDto_WhenFound()
        {
            // Arrange
            var manager = new BuildingManager { ManagerID = "M1", FullName = "Test" };
            _mockManagerRepo.Setup(r => r.GetByIdAsync("M1")).ReturnsAsync(manager);

            // Act
            var result = await _service.GetManagerByIdAsync("M1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("M1", result.ManagerID);
        }

        [Fact]
        public async Task GetManagerByIdAsync_ReturnsNull_WhenNotFound()
        {
            _mockManagerRepo.Setup(r => r.GetByIdAsync("M1")).ReturnsAsync((BuildingManager)null);
            var result = await _service.GetManagerByIdAsync("M1");
            Assert.Null(result);
        }

        #endregion

        #region GetDashboardStatsAsync

        [Fact]
        public async Task GetDashboardStatsAsync_Success_ReturnsAggregatedData()
        {
            // Arrange
            string accId = "acc1";
            string mgrId = "mgr1";
            var manager = new BuildingManager { ManagerID = mgrId, AccountID = accId };

            _mockManagerRepo.Setup(r => r.GetByAccountIdAsync(accId)).ReturnsAsync(manager);

            // Mock individual stats
            _mockRoomRepo.Setup(r => r.GetRoomCountsByManagerIdAsync(mgrId))
                .ReturnsAsync((Total: 100, Available: 20));

            _mockBillRepo.Setup(r => r.GetUnpaidBillStatsByManagerIdAsync(mgrId))
                .ReturnsAsync((Count: 5, TotalAmount: 500000));

            _mockStudentRepo.Setup(r => r.CountStudentByManagerIdAsync(mgrId))
                .ReturnsAsync(50);

            _mockMaintenanceRepo.Setup(r => r.CountUnresolveRequestsByManagerIdAsync(mgrId))
                .ReturnsAsync(3);

            // Act
            var result = await _service.GetDashboardStatsAsync(accId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Equal(100, result.Data.CountRooms);
            Assert.Equal(20, result.Data.AvailableRooms);
            Assert.Equal(5, result.Data.UnpaidUtilityBills);
            Assert.Equal(500000, result.Data.TotalUnpaidAmount);
            Assert.Equal(50, result.Data.TotalStudents);
            Assert.Equal(3, result.Data.UnResolveRequests);
        }

        [Fact]
        public async Task GetDashboardStatsAsync_Returns404_WhenManagerNotFound()
        {
            _mockManagerRepo.Setup(r => r.GetByAccountIdAsync("acc1")).ReturnsAsync((BuildingManager)null);

            var result = await _service.GetDashboardStatsAsync("acc1");

            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
        }

        #endregion

        #region GetReceiptsAsync

        [Fact]
        public async Task GetReceiptsAsync_Success_MapsComplexData()
        {
            // Arrange
            var request = new GetReceiptRequest { AccountId = "acc1", PageIndex = 1, PageSize = 10 };
            var manager = new BuildingManager { ManagerID = "mgr1" };

            // Setup nested object for projection testing
            var room = new Room { RoomName = "Room 101" };
            var contract = new Contract { ContractStatus = "Active", Room = room };
            var student = new Student { FullName = "Student A", Contracts = new List<Contract> { contract } };
            var receipts = new List<Receipt>
            {
                new Receipt
                {
                    ReceiptID = "R1",
                    Amount = 100,
                    Student = student, 
                    PaymentType = "Cash"
                },
                new Receipt
                {
                    ReceiptID = "R2",
                    Amount = 200,
                    Student = student,
                    PaymentType = "Cash"
                }

            };

            var pagedData = new PagedResult<Receipt>(receipts, 2, 1, 10);

            _mockManagerRepo.Setup(r => r.GetByAccountIdAsync(request.AccountId)).ReturnsAsync(manager);
            _mockReceiptRepo.Setup(r => r.GetReceiptsByManagerPagedAsync("mgr1", 1, 10))
                .ReturnsAsync(pagedData);

            // Act
            var result = await _service.GetReceiptsAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Items.Count);
            var dto = result.Data.Items.First();
            Assert.Equal("Room 101", dto.RoomName); // Verify deep mapping
            Assert.Equal("Student A", dto.StudentName);
        }

        [Fact]
        public async Task GetReceiptsAsync_Returns404_WhenManagerNotFound()
        {
            _mockManagerRepo.Setup(r => r.GetByAccountIdAsync(It.IsAny<string>())).ReturnsAsync((BuildingManager)null);

            var result = await _service.GetReceiptsAsync(new GetReceiptRequest());

            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
        }

        #endregion

        #region CreateManagerAsync

        [Fact]
        public async Task CreateManagerAsync_Success_CommitsTransaction()
        {
            // Arrange
            var dto = new CreateManagerDto
            {
                Email = "test@email.com",
                Password = "123",
                FullName = "New Mgr"
            };

            // Act
            var result = await _service.CreateManagerAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);

            // Verify add Account & Manager
            _mockAccountRepo.Verify(r => r.Add(It.Is<Account>(a => a.Email == dto.Email)), Times.Once);
            _mockManagerRepo.Verify(r => r.Add(It.Is<BuildingManager>(m => m.FullName == dto.FullName)), Times.Once);

            // Verify Commit
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateManagerAsync_Exception_Returns500()
        {
            // Arrange
            var dto = new CreateManagerDto { Email = "fail@test.com" };
            _mockAccountRepo.Setup(r => r.Add(It.IsAny<Account>())).Throws(new Exception("DB Fail"));

            // Act
            var result = await _service.CreateManagerAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            // Note: In CreateManagerAsync implementation provided, explicit Rollback isn't called in catch block, 
            // relying on UoW disposal or scope? 
            // However, verify logic usually checks if Commit was NOT called.
            _mockUow.Verify(u => u.CommitAsync(), Times.Never);
        }

        #endregion

        #region UpdateManagerAsync

        [Fact]
        public async Task UpdateManagerAsync_Success_UpdatesAndCommits()
        {
            // Arrange
            var dto = new UpdateBuildingManagerDto { ManagerID = "M1", FullName = "Updated Name" };
            var existing = new BuildingManager { ManagerID = "M1", FullName = "Old Name" };

            _mockManagerRepo.Setup(r => r.GetByIdAsync("M1")).ReturnsAsync(existing);

            // Act
            var result = await _service.UpdateManagerAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Updated Name", existing.FullName);
            _mockManagerRepo.Verify(r => r.Update(existing), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateManagerAsync_Returns404_WhenManagerNotFound()
        {
            _mockManagerRepo.Setup(r => r.GetByIdAsync("M1")).ReturnsAsync((BuildingManager)null);

            var result = await _service.UpdateManagerAsync(new UpdateBuildingManagerDto { ManagerID = "M1" });

            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
        }

        #endregion

        #region DeleteManagerAsync

        [Fact]
        public async Task DeleteManagerAsync_Success_WhenNoBuildingsAssigned()
        {
            // Arrange
            var manager = new BuildingManager { ManagerID = "M1", AccountID = "Acc1" };
            var account = new Account { UserId = "Acc1" };

            _mockManagerRepo.Setup(r => r.GetByIdAsync("M1")).ReturnsAsync(manager);

            // Logic code: if (buildings != null) -> return 400.
            _mockBuildingRepo.Setup(r => r.GetByManagerId("M1")).ReturnsAsync((Building?)null);

            _mockAccountRepo.Setup(r => r.GetByIdAsync("Acc1")).ReturnsAsync(account);

            // Act
            var result = await _service.DeleteManagerAsync("M1");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);

            _mockAccountRepo.Verify(r => r.Delete(account), Times.Once);
            _mockManagerRepo.Verify(r => r.Delete(manager), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteManagerAsync_Returns400_WhenBuildingsAssigned_AndRollbacks()
        {
            // Arrange
            var manager = new BuildingManager { ManagerID = "M1" };
            var buildings = new Building() ;

            _mockManagerRepo.Setup(r => r.GetByIdAsync("M1")).ReturnsAsync(manager);
            // Mock returning a list (not null)
            _mockBuildingRepo.Setup(r => r.GetByManagerId("M1")).ReturnsAsync(buildings);

            // Act
            var result = await _service.DeleteManagerAsync("M1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Cannot delete", result.Message);
            _mockUow.Verify(u => u.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteManagerAsync_Returns404_WhenManagerNotFound()
        {
            _mockManagerRepo.Setup(r => r.GetByIdAsync("M1")).ReturnsAsync((BuildingManager)null);

            var result = await _service.DeleteManagerAsync("M1");

            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            _mockUow.Verify(u => u.RollbackAsync(), Times.Once);
        }

        #endregion
    }
}