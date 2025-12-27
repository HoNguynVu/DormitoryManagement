using System;

namespace BusinessObject.DTOs.RoomDTOs
{
    public record RoomResponseDto
    {
        public string RoomID { get; init; } = string.Empty;
        public string RoomName { get; init; } = string.Empty;
        public string BuildingID { get; init; } = string.Empty;
        public string BuildingName { get; init; } = string.Empty;
        public string? RoomTypeID { get; init; }
        public string? RoomTypeName { get; init; }
        public int Capacity { get; init; }
        public int CurrentOccupancy { get; init; }
        public string RoomStatus { get; init; } = string.Empty;
        public bool IsUnderMaintenance { get; init; }
        public bool IsBeingCleaned { get; init; }
    }
}
