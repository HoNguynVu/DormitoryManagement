using API.Services.Implements;
using API.UnitOfWorks;
using BusinessObject.DTOs.RoomTypeDTOs;
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
    public class RoomTypeServiceTests
    {
        private readonly Mock<IRoomTypeUow> _mockUow;
        private readonly Mock<IRoomTypeRepository> _mockRoomTypeRepo;
        private readonly Mock<IRoomRepository> _mockRoomRepo;
        private readonly RoomTypeService _service;

        public RoomTypeServiceTests()
        {
            // 1. Khởi tạo các Mock
            _mockUow = new Mock<IRoomTypeUow>();
            _mockRoomTypeRepo = new Mock<IRoomTypeRepository>();
            _mockRoomRepo = new Mock<IRoomRepository>();

            // 2. Setup UoW trả về các Repository tương ứng
            _mockUow.Setup(u => u.RoomTypes).Returns(_mockRoomTypeRepo.Object);
            _mockUow.Setup(u => u.Rooms).Returns(_mockRoomRepo.Object);

            // 3. Setup Transaction mặc định
            _mockUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

            // 4. Khởi tạo Service
            _service = new RoomTypeService(_mockUow.Object);
        }

        #region GetAllRoomTypesAsync

        [Fact(DisplayName = "GetAllRoomTypes: Trả về danh sách RoomTypes thành công")]
        public async Task GetAllRoomTypesAsync_Success_ReturnsList()
        {
            // Arrange
            var roomTypes = new List<RoomType>
            {
                new RoomType { RoomTypeID = "RT1", TypeName = "Standard", Price = 100 },
                new RoomType { RoomTypeID = "RT2", TypeName = "VIP", Price = 200 }
            };
            _mockRoomTypeRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(roomTypes);

            // Act
            var result = await _service.GetAllRoomTypesAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(2, result.Item4.Count());
        }

        [Fact(DisplayName = "GetAllRoomTypes: Trả về lỗi 500 khi gặp Exception")]
        public async Task GetAllRoomTypesAsync_Exception_ReturnsError()
        {
            // Arrange
            _mockRoomTypeRepo.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("DB Error"));

            // Act
            var result = await _service.GetAllRoomTypesAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            Assert.Contains("DB Error", result.Message);
        }

        #endregion

        #region CreateRoomTypeAsync

        [Fact(DisplayName = "CreateRoomType: Tạo mới thành công và Commit")]
        public async Task CreateRoomTypeAsync_Success_CreatesAndCommits()
        {
            // Arrange
            var dto = new CreateRoomTypeDTO
            {
                TypeName = "Deluxe",
                Capacity = 4,
                Price = 500,
                Description = "New Room"
            };

            // Act
            var result = await _service.CreateRoomTypeAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);

            // Verify Add được gọi với ID được sinh ra (bắt đầu bằng RT-)
            _mockRoomTypeRepo.Verify(r => r.Add(It.Is<RoomType>(rt =>
                rt.TypeName == dto.TypeName &&
                rt.RoomTypeID.StartsWith("RT-")
            )), Times.Once);

            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "CreateRoomType: Rollback khi gặp lỗi DB")]
        public async Task CreateRoomTypeAsync_Exception_Rollbacks()
        {
            // Arrange
            var dto = new CreateRoomTypeDTO { TypeName = "ErrorType" };
            _mockUow.Setup(u => u.CommitAsync()).ThrowsAsync(new Exception("Commit Failed"));

            // Act
            var result = await _service.CreateRoomTypeAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            _mockUow.Verify(u => u.RollbackAsync(), Times.Once);
        }

        #endregion

        #region UpdateRoomTypeAsync

        [Fact(DisplayName = "UpdateRoomType: Trả về 404 nếu không tìm thấy RoomType")]
        public async Task UpdateRoomTypeAsync_NotFound_Returns404()
        {
            // Arrange
            var dto = new UpdateRoomTypeDTO { TypeID = "RT_NON_EXIST" };
            _mockRoomTypeRepo.Setup(r => r.GetByIdAsync(dto.TypeID)).ReturnsAsync((RoomType)null);

            // Act
            var result = await _service.UpdateRoomTypeAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact(DisplayName = "UpdateRoomType: Trả về 400 nếu Capacity mới nhỏ hơn số người đang ở hiện tại")]
        public async Task UpdateRoomTypeAsync_CapacityConflict_Returns400()
        {
            // Arrange
            var dto = new UpdateRoomTypeDTO { TypeID = "RT1", Capacity = 2 }; // Muốn giảm xuống 2
            var existingType = new RoomType { RoomTypeID = "RT1", Capacity = 4 };

            // Có 1 phòng thuộc loại này đang có 3 người ở
            var rooms = new List<Room>
            {
                new Room { RoomID = "R1", CurrentOccupancy = 3, RoomName = "R101" }
            };

            _mockRoomTypeRepo.Setup(r => r.GetByIdAsync("RT1")).ReturnsAsync(existingType);
            _mockRoomRepo.Setup(r => r.GetRoomsByTypeIdAsync("RT1")).ReturnsAsync(rooms);

            // Act
            var result = await _service.UpdateRoomTypeAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("exceeds the new capacity", result.Message);
        }

        [Fact(DisplayName = "UpdateRoomType: Cập nhật thành công RoomType và Capacity của các Room con")]
        public async Task UpdateRoomTypeAsync_Success_UpdatesTypeAndRooms()
        {
            // Arrange
            var dto = new UpdateRoomTypeDTO
            {
                TypeID = "RT1",
                Capacity = 5,
                TypeName = "Updated Name",
                Price = 999
            };

            var existingType = new RoomType { RoomTypeID = "RT1", Capacity = 4, TypeName = "Old" };

            // Các phòng con
            var rooms = new List<Room>
            {
                new Room { RoomID = "R1", Capacity = 4, CurrentOccupancy = 2 },
                new Room { RoomID = "R2", Capacity = 4, CurrentOccupancy = 0 }
            };

            _mockRoomTypeRepo.Setup(r => r.GetByIdAsync("RT1")).ReturnsAsync(existingType);
            _mockRoomRepo.Setup(r => r.GetRoomsByTypeIdAsync("RT1")).ReturnsAsync(rooms);

            // Act
            var result = await _service.UpdateRoomTypeAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);

            // 1. Kiểm tra thông tin RoomType đã được cập nhật
            Assert.Equal("Updated Name", existingType.TypeName);
            Assert.Equal(999, existingType.Price);

            // 2. Kiểm tra Capacity của các phòng con đã được cập nhật theo
            _mockRoomRepo.Verify(r => r.Update(It.Is<Room>(rm => rm.Capacity == 5)), Times.Exactly(2));

            // 3. Kiểm tra gọi Update RoomType
            _mockRoomTypeRepo.Verify(r => r.Update(existingType), Times.Once);

            // 4. Commit transaction
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        #endregion

        #region DeleteRoomTypeAsync

        [Fact(DisplayName = "DeleteRoomType: Trả về 404 nếu không tìm thấy RoomType")]
        public async Task DeleteRoomTypeAsync_NotFound_Returns404()
        {
            // Arrange
            _mockRoomTypeRepo.Setup(r => r.GetByIdAsync("RT_X")).ReturnsAsync((RoomType)null);

            // Act
            var result = await _service.DeleteRoomTypeAsync("RT_X");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact(DisplayName = "DeleteRoomType: Trả về 400 nếu vẫn còn phòng sử dụng loại này")]
        public async Task DeleteRoomTypeAsync_HasAssociatedRooms_Returns400()
        {
            // Arrange
            var existingType = new RoomType { RoomTypeID = "RT1" };
            _mockRoomTypeRepo.Setup(r => r.GetByIdAsync("RT1")).ReturnsAsync(existingType);

            // Giả lập vẫn còn phòng
            _mockRoomRepo.Setup(r => r.HasAnyRoomByTypeAsync("RT1")).ReturnsAsync(true);

            // Act
            var result = await _service.DeleteRoomTypeAsync("RT1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("associated with this type", result.Message);

            // Đảm bảo không gọi xóa
            _mockRoomTypeRepo.Verify(r => r.Delete(It.IsAny<RoomType>()), Times.Never);
        }

        [Fact(DisplayName = "DeleteRoomType: Xóa thành công khi không còn ràng buộc")]
        public async Task DeleteRoomTypeAsync_Success_DeletesEntity()
        {
            // Arrange
            var existingType = new RoomType { RoomTypeID = "RT1" };
            _mockRoomTypeRepo.Setup(r => r.GetByIdAsync("RT1")).ReturnsAsync(existingType);
            _mockRoomRepo.Setup(r => r.HasAnyRoomByTypeAsync("RT1")).ReturnsAsync(false);

            // Act
            var result = await _service.DeleteRoomTypeAsync("RT1");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);

            _mockRoomTypeRepo.Verify(r => r.Delete(existingType), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "DeleteRoomType: Rollback khi gặp lỗi DB")]
        public async Task DeleteRoomTypeAsync_Exception_Rollbacks()
        {
            // Arrange
            var existingType = new RoomType { RoomTypeID = "RT1" };
            _mockRoomTypeRepo.Setup(r => r.GetByIdAsync("RT1")).ReturnsAsync(existingType);
            _mockRoomRepo.Setup(r => r.HasAnyRoomByTypeAsync("RT1")).ReturnsAsync(false);

            // Giả lập lỗi khi commit
            _mockUow.Setup(u => u.CommitAsync()).ThrowsAsync(new Exception("Error"));

            // Act
            var result = await _service.DeleteRoomTypeAsync("RT1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(500, result.StatusCode);
            _mockUow.Verify(u => u.RollbackAsync(), Times.Once);
        }

        #endregion
    }
}