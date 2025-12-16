namespace BusinessObject.DTOs.ReportDTOs
{
    public record EquipmentStatusDto
    {
        public string EquipmentID { get; init; } = string.Empty;
        public string EquipmentName { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string RoomID { get; init; } = string.Empty;
        public string RoomName { get; init; } = string.Empty;
    }
}
