using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.RoomDTOs;

namespace API.Services.Implements
{
    public class RoomService : IRoomService
    {
        private readonly IRoomUow _roomUow;
        public RoomService(IRoomUow roomUow)
        {
            _roomUow = roomUow;
        }

        public async Task<(bool Success, string Message, int StatusCode, IEnumerable<RegisRoomDTOs>)> GetRoomForRegistration()
        {
            try
            {
                // 1. Lấy tất cả phòng kèm loại phòng (1 Query)
                var rooms = await _roomUow.Rooms.GetAllRoomsWithTypesAsync();

                // 2. Lấy tất cả số lượng đơn đang Pending (1 Query)
                var pendingCountsDict = await _roomUow.RegistrationForms.CountPendingFormsByRoomAsync();

                var regisRoomDTOs = new List<RegisRoomDTOs>();

                foreach (var room in rooms)
                {
                    // Tra cứu số lượng Pending từ Dictionary (trên RAM, cực nhanh)
                    // Nếu không tìm thấy (GetValueOrDefault) thì trả về 0
                    int pendingCount = pendingCountsDict.GetValueOrDefault(room.RoomID, 0);

                    // Tính toán hiển thị cho Frontend
                    // RegisteredOccupancy ở đây hiểu là số lượng ĐANG GIỮ CHỖ

                    regisRoomDTOs.Add(new RegisRoomDTOs
                    {
                        RoomId = room.RoomID,
                        RoomName = room.RoomName,
                        // Vì đã Include ở Repo nên không cần query lại RoomType
                        RoomType = room.RoomType?.TypeName ?? "Unknown",
                        Price = room.RoomType?.Price ?? 0,
                        Capacity = room.Capacity,
                        CurrentOccupancy = room.CurrentOccupancy, // Số người đang ở thực tế (Contract Active)
                        RegisteredOccupancy = pendingCount // Số người đang chờ thanh toán
                    });
                }

                return (true, "Rooms retrieved successfully", 200, regisRoomDTOs);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}", 500, Enumerable.Empty<RegisRoomDTOs>());
            }
        }
    }
}
