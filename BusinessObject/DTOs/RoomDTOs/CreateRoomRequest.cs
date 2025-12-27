using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs.RoomDTOs
{
    public class CreateRoomRequest
    {
        [Required]
        public string BuildingId { get; set; } = string.Empty;

        [Required]
        public string RoomTypeId { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public bool IsUnderMaintenance { get; set; } = false;
        public bool IsBeingCleaned { get; set; } = false;
        public string RoomName { get; set; } = string.Empty;
    }
}
