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

        public string RoomName { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string Status { get; set; } = "Available";
    }
}
