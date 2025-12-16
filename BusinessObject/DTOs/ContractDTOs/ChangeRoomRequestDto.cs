using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs.ContractDTOs
{
    public class ChangeRoomRequestDto
    {
        [Required]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        public string NewRoomId { get; set; } = string.Empty;

        [Required]
        public ChangeRoomReasonEnum Reason { get; set; }

        public string? ManagerNote { get; set; }
    }
}
