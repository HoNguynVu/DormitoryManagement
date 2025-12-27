using BusinessObject.DTOs.RoomDTOs;
using BusinessObject.Entities;

namespace BusinessObject.Factories
{
    public static class RoomFactory
    {
        public static Room Create(CreateRoomRequest request, RoomType roomType)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (roomType == null) throw new ArgumentNullException(nameof(roomType));

            var roomName = (request.RoomName ?? string.Empty).Trim();
            var roomId = $"RM-{request.BuildingId}-{roomName}";

            return new Room
            {
                RoomID = roomId,
                RoomName = roomName,
                BuildingID = request.BuildingId,
                RoomTypeID = request.RoomTypeId,
                Capacity = roomType.Capacity,
                CurrentOccupancy = 0,
                RoomStatus = string.IsNullOrWhiteSpace(request.Status) ? "Available" : request.Status,
                IsUnderMaintenance = false,
                IsBeingCleaned = false
            };
        }
    }
}
