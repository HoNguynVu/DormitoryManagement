using API.Services.Helpers;
using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.Entities;

namespace API.Services.Implements
{
    public class RoomEquipmentService : IRoomEquipmentService
    {
        private readonly IRoomEquipmentUow _uow;
        public RoomEquipmentService(IRoomEquipmentUow uow)
        {
            _uow = uow;
        }
        public async Task<(bool Success, string Message, int StatusCode)> ChangeStatusAsync(string roomId, string equipmentId, int quantity, string fromStatus, string toStatus)
        {
            try
            {
                // Validate đầu vào
                if (quantity <= 0)
                {
                    return (false, "Số lượng phải lớn hơn 0.", 400); // 400 Bad Request
                }

                if (fromStatus == toStatus)
                {
                    return (false, "Trạng thái mới phải khác trạng thái cũ.", 400);
                }
                // TÌM NGUỒN 
                var sourceItem = await _uow.RoomEquipments.GetRoomEquipmentByStatusAsync(roomId, equipmentId, fromStatus);
                if (sourceItem == null)
                {
                    return (false, $"Không tìm thấy thiết bị ở trạng thái '{fromStatus}' trong phòng này.", 404); // 404 Not Found
                }

                if (sourceItem.Quantity < quantity)
                {
                    return (false, $"Số lượng không đủ để chuyển đổi (Hiện có: {sourceItem.Quantity}).", 400);
                }


                // TÌM ĐÍCH 
                var destItem = await _uow.RoomEquipments.GetRoomEquipmentByStatusAsync(roomId, equipmentId, toStatus);

                // THỰC HIỆN CHUYỂN ĐỔI

                sourceItem.Quantity -= quantity;
                if (sourceItem.Quantity == 0)
                {
                    _uow.RoomEquipments.Delete(sourceItem);
                }
                else
                {
                    _uow.RoomEquipments.Update(sourceItem);
                }
                if (destItem == null)
                {
                    destItem = new RoomEquipment
                    {
                        RoomEquipmentID = "RO-EQ" + IdGenerator.GenerateUniqueSuffix(),
                        RoomID = roomId,
                        EquipmentID = equipmentId,
                        Quantity = quantity,
                        Status = toStatus
                    };
                    _uow.RoomEquipments.Add(destItem);
                }
                else
                {
                    destItem.Quantity += quantity;
                    _uow.RoomEquipments.Update(destItem);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi hệ thống: {ex.Message}", 500); 
            }
        }
    }
}
