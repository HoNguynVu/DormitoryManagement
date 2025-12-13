namespace BusinessObject.DTOs.RoomDTOs
{
    public record RoomStatusDto
    {
        public string RoomID { get; init; } = string.Empty;
        public int Capacity { get; init; }
        public int Occupied { get; init; }
        public int AvailableBeds { get; init; }
        public string RoomStatus { get; init; } = string.Empty;
    }
}
