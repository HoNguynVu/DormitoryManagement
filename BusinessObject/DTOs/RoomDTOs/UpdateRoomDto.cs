using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs.RoomDTOs
{
    public record UpdateRoomDto
    {
        public string RoomID { get; init; } = string.Empty;

        public string? Gender { get; init; }

        public string? RoomStatus { get; init; }

        public bool? IsUnderMaintenance { get; init; }

        public bool? IsBeingCleaned { get; init; }
    }
}
