using API.Services.Implements;
using API.UnitOfWorks;
using BusinessObject.DTOs.RoomDTOs;
using BusinessObject.Entities;
using DataAccess.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions; 
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Services.Implements
{
    public class RoomServiceTests
    {
        private readonly Mock<IRoomUow> _mockUow;
        private readonly Mock<IRoomRepository> _mockRoomRepo;
        private readonly Mock<IRegistrationFormRepository> _mockRegRepo;
        private readonly Mock<IContractRepository> _mockContractRepo;
        private readonly Mock<IRoomTypeRepository> _mockRoomTypeRepo;
        private readonly Mock<IBuildingManagerRepository> _mockManagerRepo;

        private readonly RoomService _service;

        public RoomServiceTests()
        {
            // 1. Init Mocks
            _mockUow = new Mock<IRoomUow>();
            _mockRoomRepo = new Mock<IRoomRepository>();
            _mockRegRepo = new Mock<IRegistrationFormRepository>();
            _mockContractRepo = new Mock<IContractRepository>();
            _mockRoomTypeRepo = new Mock<IRoomTypeRepository>();
            _mockManagerRepo = new Mock<IBuildingManagerRepository>();

            // 2. Setup UoW Returns
            _mockUow.Setup(u => u.Rooms).Returns(_mockRoomRepo.Object);
            _mockUow.Setup(u => u.RegistrationForms).Returns(_mockRegRepo.Object);
            _mockUow.Setup(u => u.Contracts).Returns(_mockContractRepo.Object);
            _mockUow.Setup(u => u.RoomTypes).Returns(_mockRoomTypeRepo.Object);
            _mockUow.Setup(u => u.BuildingManagers).Returns(_mockManagerRepo.Object);

            // 3. Setup Transaction Defaults
            _mockUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // 4. Init Service
            _service = new RoomService(_mockUow.Object);
        }

        #region GetRoomForRegistration

        [Fact(DisplayName = "GetRoomForRegistration: Loại bỏ phòng đã đầy (Full Capacity)")]
        public async Task GetRoomForRegistration_ExcludesFullRooms()
        {
            // Arrange
            var filter = new RoomFilterDto();

            // Phòng 1: Capacity 4, Occupancy 4 -> Full
            var roomFull = new Room
            {
                RoomID = "R1",
                Capacity = 4,
                CurrentOccupancy = 4,
                RoomType = new RoomType { TypeName = "Standard", Price = 100 },
                Building = new Building { BuildingName = "A" }
            };

            // Phòng 2: Capacity 4, Occupancy 2 -> Available
            var roomAvail = new Room
            {
                RoomID = "R2",
                Capacity = 4,
                CurrentOccupancy = 2,
                RoomType = new RoomType { TypeName = "Standard", Price = 100 },
                Building = new Building { BuildingName = "A" }
            };

            // --- QUAN TRỌNG: Setup Mock cho FindBySpecificationAsync ---
            // Chúng ta dùng It.IsAny<Expression<Func<Room, bool>>>() để chấp nhận bất kỳ expression nào
            _mockRoomRepo.Setup(r => r.FindBySpecificationAsync(It.IsAny<Expression<Func<Room, bool>>>()))
                .ReturnsAsync(new List<Room> { roomFull, roomAvail });

            // Giả lập Dictionary trả về số đơn pending
            var pendingDict = new Dictionary<string, int> { { "R2", 1 } };
            _mockRegRepo.Setup(r => r.CountPendingFormsByRoomAsync()).ReturnsAsync(pendingDict);

            // Act
            var result = await _service.GetRoomForRegistration(filter);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Item4); // Chỉ trả về 1 phòng (R2)
            Assert.Equal("R2", result.Item4.First().RoomId);
            Assert.Equal(1, result.Item4.First().RegisteredOccupancy);
        }

        #endregion

        #region GetAvailableRoomsAsync

        [Fact(DisplayName = "GetAvailableRooms: Lọc bỏ phòng hết chỗ nếu OnlyAvailable = true")]
        public async Task GetAvailableRoomsAsync_OnlyAvailable_FiltersCorrectly()
        {
            // Arrange
            var filter = new RoomFilterDto { OnlyAvailable = true };

            var roomFull = new Room { RoomID = "R1", Capacity = 2, CurrentOccupancy = 2 };
            var roomEmpty = new Room { RoomID = "R2", Capacity = 2, CurrentOccupancy = 0, RoomType = new RoomType { Price = 500, TypeName = "VIP" } };

            // --- QUAN TRỌNG: Setup Mock cho FindBySpecificationAsync ---
            _mockRoomRepo.Setup(r => r.FindBySpecificationAsync(It.IsAny<Expression<Func<Room, bool>>>()))
                .ReturnsAsync(new List<Room> { roomFull, roomEmpty });

            _mockRegRepo.Setup(r => r.CountPendingFormsByRoomAsync())
                .ReturnsAsync(new Dictionary<string, int>());
            _mockContractRepo.Setup(c => c.CountActiveContractsByRoomAsync())
                .ReturnsAsync(new Dictionary<string, int>());

            // Act
            var result = await _service.GetAvailableRoomsAsync(filter);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Item4); // Chỉ còn 1 phòng trống
            Assert.Equal("R2", result.Item4.First().RoomID);
        }

        #endregion

        // --- Các Test dưới đây giữ nguyên vì không dùng Specification ---

        #region CreateRoomAsync

        [Fact(DisplayName = "CreateRoom: Trả về lỗi 400 nếu Building không tồn tại")]
        public async Task CreateRoomAsync_BuildingNotFound_Returns400()
        {
            var req = new CreateRoomRequest { BuildingId = "B1", RoomTypeId = "RT1" };
            _mockUow.Setup(u => u.BuildingExistsAsync("B1")).ReturnsAsync(false);
            var result = await _service.CreateRoomAsync(req);
            Assert.False(result.Success);
            Assert.Equal("Building not found", result.Message);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact(DisplayName = "CreateRoom: Thành công, thêm phòng mới và Commit Transaction")]
        public async Task CreateRoomAsync_Success_CommitsTransaction()
        {
            var req = new CreateRoomRequest { BuildingId = "B1", RoomTypeId = "RT1", RoomName = "101"};
            _mockUow.Setup(u => u.BuildingExistsAsync("B1")).ReturnsAsync(true);
            _mockRoomTypeRepo.Setup(r => r.GetByIdAsync("RT1")).ReturnsAsync(new RoomType { RoomTypeID = "RT1" });
            _mockRoomRepo.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((Room)null);

            var result = await _service.CreateRoomAsync(req);

            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);
            _mockRoomRepo.Verify(r => r.Add(It.IsAny<Room>()), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        #endregion

        #region UpdateRoomAsync

        [Fact(DisplayName = "UpdateRoom: Cập nhật thành công trạng thái")]
        public async Task UpdateRoomAsync_Success_UpdatesStatus()
        {
            var dto = new UpdateRoomDto { RoomID = "R1", IsUnderMaintenance = true, Gender = "Female" };
            var existingRoom = new Room { RoomID = "R1", IsUnderMaintenance = false, RoomStatus = "Active" };
            _mockRoomRepo.Setup(r => r.GetByIdAsync("R1")).ReturnsAsync(existingRoom);

            var result = await _service.UpdateRoomAsync(dto);

            Assert.True(result.Success);
            Assert.True(existingRoom.IsUnderMaintenance);
            _mockRoomRepo.Verify(r => r.Update(existingRoom), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        #endregion

        #region DeleteRoomAsync

        [Fact(DisplayName = "DeleteRoom: Xóa thành công")]
        public async Task DeleteRoomAsync_Success()
        {
            var room = new Room { RoomID = "R1" };
            _mockRoomRepo.Setup(r => r.GetByIdAsync("R1")).ReturnsAsync(room);

            var result = await _service.DeleteRoomAsync("R1");

            Assert.True(result.Success);
            _mockRoomRepo.Verify(r => r.Delete(room), Times.Once);
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        #endregion

        #region GetRoomStatusAsync

        [Fact(DisplayName = "GetRoomStatus: Tính toán Occupied và Available Beds chính xác")]
        public async Task GetRoomStatusAsync_CalculatesCorrectly()
        {
            string roomId = "R1";
            var room = new Room { RoomID = roomId, Capacity = 10, CurrentOccupancy = 2 };

            _mockRoomRepo.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
            _mockContractRepo.Setup(c => c.CountContractsByRoomIdAndStatus(roomId, "Active")).ReturnsAsync(3);
            _mockRegRepo.Setup(r => r.CountRegistrationFormsByRoomId(roomId)).ReturnsAsync(1);

            var result = await _service.GetRoomStatusAsync(roomId);

            Assert.True(result.Success);
            Assert.Equal(3, result.Item4.Occupied); // Max(2, 3)
            Assert.Equal(6, result.Item4.AvailableBeds); // 10 - (3 + 1)
        }

        #endregion

        #region GetAllRoomsForManagerAsync

        [Fact(DisplayName = "GetForManager: Trả về danh sách phòng theo ManagerID")]
        public async Task GetAllRoomsForManagerAsync_Success()
        {
            string accId = "acc1";
            var manager = new BuildingManager { ManagerID = "MGR1" };
            var rooms = new List<Room>
            {
                new Room { RoomID = "R1", Building = new Building { BuildingName = "B1" } }
            };

            _mockManagerRepo.Setup(m => m.GetByAccountIdAsync(accId)).ReturnsAsync(manager);
            _mockRoomRepo.Setup(r => r.GetRoomByManagerIdAsync("MGR1")).ReturnsAsync(rooms);

            var result = await _service.GetAllRoomsForManagerAsync(accId);

            Assert.True(result.Success);
            Assert.Single(result.Item4);
            Assert.Equal("B1", result.Item4.First().BuildingName);
        }

        #endregion
    }
}