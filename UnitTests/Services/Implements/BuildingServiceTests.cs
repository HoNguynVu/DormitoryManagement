using API.Services.Implements;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.BuildingDTOs;
using BusinessObject.DTOs.RoomDTOs;
using BusinessObject.Entities;
using DataAccess.Interfaces;
using DocumentFormat.OpenXml.ExtendedProperties;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Services.Implements
{
    public class BuildingServiceTests
    {
        private readonly Mock<IBuildingUow> _mockUow;
        private readonly Mock<IRoomService> _mockRoomService;

        // Mock các Repository con bên trong UOW
        private readonly Mock<IBuildingRepository> _mockBuildingRepo;
        private readonly Mock<IBuildingManagerRepository> _mockManagerRepo;
        private readonly Mock<IRoomRepository> _mockRoomRepo;

        private readonly BuildingService _service;

        public BuildingServiceTests()
        {
            _mockUow = new Mock<IBuildingUow>();
            _mockRoomService = new Mock<IRoomService>();

            _mockBuildingRepo = new Mock<IBuildingRepository>();
            _mockManagerRepo = new Mock<IBuildingManagerRepository>();
            _mockRoomRepo = new Mock<IRoomRepository>();

            // Setup UOW trả về các Repository mock
            _mockUow.Setup(u => u.Buildings).Returns(_mockBuildingRepo.Object);
            _mockUow.Setup(u => u.BuildingManagers).Returns(_mockManagerRepo.Object);
            _mockUow.Setup(u => u.Rooms).Returns(_mockRoomRepo.Object);

            // Setup Transaction mặc định
            _mockUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

            _service = new BuildingService(_mockUow.Object, _mockRoomService.Object);
        }

        #region GetAllBuildingAsync

        [Fact]
        public async Task GetAllBuildingAsync_ReturnsSuccess_WhenDataExists()
        {
            // Arrange
            var buildings = new List<Building>
            {
                new Building { BuildingID = "B1", BuildingName = "Building A" },
                new Building { BuildingID = "B2", BuildingName = "Building B" }
            };
            _mockBuildingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(buildings);

            // Act
            var result = await _service.GetAllBuildingAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(2, result.Item4.Count());
        }

        [Fact]
        public async Task GetAllBuildingAsync_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            _mockBuildingRepo.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("DB Error"));

            // Act
            var result = await _service.GetAllBuildingAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
        }

        #endregion

        #region GetBuildingWithManagerAsync

        [Fact]
        public async Task GetBuildingWithManagerAsync_ReturnsSuccess_WhenFound()
        {
            // Arrange
            var building = new Building { BuildingID = "B1", BuildingName = "A", ManagerID = "M1" };
            var manager = new BuildingManager { ManagerID = "M1", FullName = "John Doe" };

            _mockBuildingRepo.Setup(r => r.GetByIdAsync("B1")).ReturnsAsync(building);
            _mockManagerRepo.Setup(r => r.GetByIdAsync("M1")).ReturnsAsync(manager);

            // Act
            var result = await _service.GetBuildingWithManagerAsync("B1");

            // Assert
            Assert.True(result.Success);
            var dto = result.Data.First();
            Assert.Equal("John Doe", dto.ManagerName);
        }

        [Fact]
        public async Task GetBuildingWithManagerAsync_Returns404_WhenBuildingNotFound()
        {
            // Arrange
            _mockBuildingRepo.Setup(r => r.GetByIdAsync("B1")).ReturnsAsync((Building)null);

            // Act
            var result = await _service.GetBuildingWithManagerAsync("B1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Building not found.", result.Message);
        }

        [Fact]
        public async Task GetBuildingWithManagerAsync_Returns404_WhenManagerNotFound()
        {
            // Arrange
            var building = new Building { BuildingID = "B1", ManagerID = "M1" };
            _mockBuildingRepo.Setup(r => r.GetByIdAsync("B1")).ReturnsAsync(building);
            _mockManagerRepo.Setup(r => r.GetByIdAsync("M1")).ReturnsAsync((BuildingManager)null);

            // Act
            var result = await _service.GetBuildingWithManagerAsync("B1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("Manager not found", result.Message);
        }

        #endregion

        #region GetRoomByManagerId

        [Fact]
        public async Task GetRoomByManagerId_ReturnsSuccess_WhenManagerExists()
        {
            // Arrange
            var manager = new BuildingManager { ManagerID = "M1", AccountID = "ACC1" };
            var roomList = new List<RoomResponseDto> { new RoomResponseDto { RoomID = "R1" } };

            _mockManagerRepo.Setup(r => r.GetByIdAsync("M1")).ReturnsAsync(manager);

            // Mock dependency service RoomService
            _mockRoomService.Setup(s => s.GetAllRoomsForManagerAsync("ACC1"))
                .ReturnsAsync((true, "Success", 200, roomList));

            // Act
            var result = await _service.GetRoomByManagerId("M1");

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetRoomByManagerId_Returns404_WhenManagerNotFound()
        {
            // Arrange
            _mockManagerRepo.Setup(r => r.GetByIdAsync("M1")).ReturnsAsync((BuildingManager)null);

            // Act
            var result = await _service.GetRoomByManagerId("M1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
        }

        #endregion

        #region CreateBuildingAsync

        [Fact]
        public async Task CreateBuildingAsync_Returns201_WhenValid()
        {
            // Arrange
            var dto = new CreateBuildingDto { BuildingName = "New Bld", ManagerID = "M1" };
            var manager = new BuildingManager { ManagerID = "M1" };

            _mockManagerRepo.Setup(r => r.GetByIdAsync("M1")).ReturnsAsync(manager);
            _mockBuildingRepo.Setup(r => r.IsManagerAssigned("M1")).ReturnsAsync(false);

            // Act
            var result = await _service.CreateBuildingAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);
            _mockBuildingRepo.Verify(r => r.Add(It.IsAny<Building>()), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateBuildingAsync_Returns404_WhenManagerNotFound_AndRollback()
        {
            // Arrange
            var dto = new CreateBuildingDto { ManagerID = "M1" };
            _mockManagerRepo.Setup(r => r.GetByIdAsync("M1")).ReturnsAsync((BuildingManager)null);

            // Act
            var result = await _service.CreateBuildingAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            _mockUow.Verify(u => u.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateBuildingAsync_Returns400_WhenManagerAlreadyAssigned()
        {
            // Arrange
            var dto = new CreateBuildingDto { ManagerID = "M1" };
            var manager = new BuildingManager { ManagerID = "M1" };

            _mockManagerRepo.Setup(r => r.GetByIdAsync("M1")).ReturnsAsync(manager);
            _mockBuildingRepo.Setup(r => r.IsManagerAssigned("M1")).ReturnsAsync(true);

            // Act
            var result = await _service.CreateBuildingAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            _mockUow.Verify(u => u.RollbackAsync(), Times.Once);
        }

        #endregion

        #region UpdateBuildingAsync

        [Fact]
        public async Task UpdateBuildingAsync_Returns200_WhenValid()
        {
            // Arrange
            var dto = new UpdateBuildingDto { BuildingID = "B1", ManagerID = "M2" };
            var building = new Building { BuildingID = "B1", ManagerID = "M1" }; // Old manager M1
            var manager2 = new BuildingManager { ManagerID = "M2" };

            _mockBuildingRepo.Setup(r => r.GetByIdAsync("B1")).ReturnsAsync(building);
            _mockManagerRepo.Setup(r => r.GetByIdAsync("M2")).ReturnsAsync(manager2);
            _mockBuildingRepo.Setup(r => r.IsManagerAssigned("M2")).ReturnsAsync(false);

            // Act
            var result = await _service.UpdateBuildingAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("M2", building.ManagerID);
            _mockBuildingRepo.Verify(r => r.Update(building), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateBuildingAsync_Returns400_WhenNewManagerAlreadyAssigned()
        {
            // Arrange
            var dto = new UpdateBuildingDto { BuildingID = "B1", ManagerID = "M2" };
            var building = new Building { BuildingID = "B1", ManagerID = "M1" };
            var manager2 = new BuildingManager { ManagerID = "M2" };

            _mockBuildingRepo.Setup(r => r.GetByIdAsync("B1")).ReturnsAsync(building);
            _mockManagerRepo.Setup(r => r.GetByIdAsync("M2")).ReturnsAsync(manager2);
            // M2 đã được gán cho tòa nhà khác
            _mockBuildingRepo.Setup(r => r.IsManagerAssigned("M2")).ReturnsAsync(true);

            // Act
            var result = await _service.UpdateBuildingAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            _mockUow.Verify(u => u.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateBuildingAsync_Returns404_WhenBuildingNotFound()
        {
            // Arrange
            _mockBuildingRepo.Setup(r => r.GetByIdAsync("B1")).ReturnsAsync((Building)null);
            var dto = new UpdateBuildingDto { BuildingID = "B1" };

            // Act
            var result = await _service.UpdateBuildingAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            _mockUow.Verify(u => u.RollbackAsync(), Times.Once);
        }

        #endregion

        #region GetBuildingsStats

        [Fact]
        public async Task GetBuildingsStats_ReturnsCorrectData()
        {
            // Arrange
            _mockRoomRepo.Setup(r => r.CountRooms()).ReturnsAsync(100);
            _mockRoomRepo.Setup(r => r.CountAvailableRooms()).ReturnsAsync(20);
            _mockRoomRepo.Setup(r => r.CountRoomsFull()).ReturnsAsync(70);
            _mockRoomRepo.Setup(r => r.CountMaintenanceRooms()).ReturnsAsync(10);

            // Act
            var result = await _service.GetBuildingsStats();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(100, result.Data.TotalRooms);
            Assert.Equal(20, result.Data.TotalAvailableRooms);
            Assert.Equal(70, result.Data.TotalFullRooms);
            Assert.Equal(10, result.Data.TotalMaintenanceRooms);
        }

        [Fact]
        public async Task GetBuildingsStats_Returns500_OnException()
        {
            // Arrange
            _mockRoomRepo.Setup(r => r.CountRooms()).ThrowsAsync(new Exception("Fail"));

            // Act
            var result = await _service.GetBuildingsStats();

            // Assert
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
        }

        #endregion
    }
}