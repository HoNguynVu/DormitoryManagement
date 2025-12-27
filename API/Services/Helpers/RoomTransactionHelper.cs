using BusinessObject.Entities;
using API.UnitOfWorks;

namespace API.Services.Helpers
{
    public static class RoomTransactionHelper
    {
        /// <summary>
        /// Logic đổi phòng: Cập nhật sĩ số phòng cũ/mới và Contract
        /// </summary>
        public static async Task SwapRoomLogicAsync(IContractUow uow, Contract activeContract, string newRoomId)
        {
            // 1. Lấy thông tin phòng
            var oldRoom = await uow.Rooms.GetByIdAsync(activeContract.RoomID);
            var newRoom = await uow.Rooms.GetByIdAsync(newRoomId);

            if (oldRoom == null || newRoom == null)
                throw new Exception("Dữ liệu phòng bị lỗi (Room data missing).");

            // 2. Kiểm tra sức chứa (Tránh Race Condition phút chót)
            // Chỉ check full nếu phòng mới KHÁC phòng hiện tại
            if (newRoom.RoomID != activeContract.RoomID && newRoom.CurrentOccupancy >= newRoom.Capacity)
            {
                throw new Exception($"Phòng {newRoom.RoomName} hiện đã đủ người, không thể chuyển vào.");
            }

            // 3. Cập nhật Phòng Cũ (Giảm sĩ số)
            if (oldRoom.CurrentOccupancy > 0)
            {
                oldRoom.CurrentOccupancy -= 1;
            }
            // Nếu phòng đang Full mà có người đi -> Thành Available
            if (oldRoom.CurrentOccupancy < oldRoom.Capacity)
            {
                oldRoom.RoomStatus = "Available";
            }
            uow.Rooms.Update(oldRoom);

            // 4. Cập nhật Phòng Mới (Tăng sĩ số)
            newRoom.CurrentOccupancy += 1;
            // Nếu phòng đầy -> Thành Full
            if (newRoom.CurrentOccupancy >= newRoom.Capacity)
            {
                newRoom.RoomStatus = "Full";
            }
            uow.Rooms.Update(newRoom);

            // 5. Cập nhật Hợp đồng
            activeContract.RoomID = newRoomId;
            uow.Contracts.Update(activeContract);

        }
    }
}
