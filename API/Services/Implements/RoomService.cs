using API.Services.Interfaces;
using API.UnitOfWorks;
using BusinessObject.DTOs.RoomDTOs;
using BusinessObject.Entities;

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

        public async Task<(bool Success, string Message, int StatusCode, RoomResponseDto?)> CreateRoomAsync(CreateRoomRequest request)
        {
            if (request is null) return (false, "Request is null", 400, null);

            // Validate building exists
            if (!await _roomUow.BuildingExistsAsync(request.BuildingId))
                return (false, "Building not found", 400, null);

            // Validate room type exists and derive capacity from it
            var roomType = await _roomUow.RoomTypes.GetRoomTypeById(request.RoomTypeId);
            if (roomType == null)
                return (false, "RoomType not found", 400, null);

            // Build canonical RoomID following your script pattern
            var roomId = $"RM_{request.BuildingId}_{request.RoomName}";

            // Ensure unique RoomID
            var existing = await _roomUow.Rooms.GetRoomById(roomId);
            if (existing != null)
                return (false, "Room already exists", 409, null);

            var room = new Room
            {
                RoomID = roomId,
                RoomName = request.RoomName,
                BuildingID = request.BuildingId,
                RoomTypeID = request.RoomTypeId,
                // Use the capacity defined by RoomType to guarantee consistency with dataset
                Capacity = roomType.Capacity,
                CurrentOccupancy = 0,
                RoomStatus = string.IsNullOrWhiteSpace(request.Status) ? "Available" : request.Status,
                IsUnderMaintenance = false,
                IsBeingCleaned = false
            };

            try
            {
                _roomUow.Rooms.AddRoom(room);
                await _roomUow.SaveChangesAsync();

                var response = new RoomResponseDto
                {
                    RoomID = room.RoomID,
                    RoomName = room.RoomName,
                    BuildingID = room.BuildingID,
                    RoomTypeID = room.RoomTypeID,
                    Capacity = room.Capacity,
                    CurrentOccupancy = room.CurrentOccupancy,
                    RoomStatus = room.RoomStatus,
                    IsUnderMaintenance = room.IsUnderMaintenance,
                    IsBeingCleaned = room.IsBeingCleaned
                };

                return (true, "Room created", 201, response);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to create room: {ex.Message}", 500, null);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode)> UpdateRoomAsync(UpdateRoomDto request)
        {
            if (request is null) return (false, "Request is null", 400);

            try
            {
                var existing = await _roomUow.Rooms.GetRoomById(request.RoomID);
                if (existing is null) return (false, "Room not found", 404);

                // Do not allow renaming building or room name because RoomID is derived and is primary key
                if (!string.IsNullOrWhiteSpace(request.BuildingID) && request.BuildingID != existing.BuildingID)
                    return (false, "Changing BuildingID is not allowed. Delete and recreate room to move it.", 400);

                if (!string.IsNullOrWhiteSpace(request.RoomName) && request.RoomName != existing.RoomName)
                    return (false, "Changing RoomName is not allowed. Delete and recreate room to rename it.", 400);

                // If room type changes, fetch it and update capacity accordingly
                if (!string.IsNullOrWhiteSpace(request.RoomTypeID) && request.RoomTypeID != existing.RoomTypeID)
                {
                    var newType = await _roomUow.RoomTypes.GetRoomTypeById(request.RoomTypeID);
                    if (newType == null) return (false, "New RoomType not found", 400);

                    // New capacity must be >= current occupancy
                    if (newType.Capacity < existing.CurrentOccupancy)
                        return (false, "New room type capacity is less than current occupancy", 400);

                    existing.RoomTypeID = newType.RoomTypeID;
                    existing.Capacity = newType.Capacity;
                }

                // If client provides explicit capacity, ensure it's not less than current occupancy
                if (request.Capacity.HasValue)
                {
                    // Capacity is defined by RoomType in this system. Do not allow ad-hoc capacity updates.
                    return (false, "Capacity is managed by RoomType and cannot be updated directly. Change RoomType to modify capacity.", 400);
                }

                if (request.CurrentOccupancy.HasValue)
                {
                    if (request.CurrentOccupancy.Value > existing.Capacity)
                        return (false, "Current occupancy cannot exceed capacity", 400);

                    existing.CurrentOccupancy = request.CurrentOccupancy.Value;
                }

                if (!string.IsNullOrWhiteSpace(request.RoomStatus)) existing.RoomStatus = request.RoomStatus;
                if (request.IsUnderMaintenance.HasValue) existing.IsUnderMaintenance = request.IsUnderMaintenance.Value;
                if (request.IsBeingCleaned.HasValue) existing.IsBeingCleaned = request.IsBeingCleaned.Value;

                _roomUow.Rooms.UpdateRoom(existing);
                await _roomUow.SaveChangesAsync();

                return (true, "Room updated", 200);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to update room: {ex.Message}", 500);
            }
        }

        public async Task<(bool Success, string Message, int StatusCode)> DeleteRoomAsync(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId)) return (false, "Invalid room id", 400);

            try
            {
                var existing = await _roomUow.Rooms.GetRoomById(roomId);
                if (existing is null) return (false, "Room not found", 404);

                _roomUow.Rooms.DeleteRoom(existing);
                await _roomUow.SaveChangesAsync();

                return (true, "Room deleted", 200);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to delete room: {ex.Message}", 500);
            }
        }
    }
}
