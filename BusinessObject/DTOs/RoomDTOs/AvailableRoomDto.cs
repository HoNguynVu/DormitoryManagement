namespace BusinessObject.DTOs.RoomDTOs
{
    public record AvailableRoomDto
    {
        public string RoomID { get; init; } = string.Empty;
        public string RoomName { get; init; } = string.Empty;
        public int Capacity { get; init; }
        public int Occupied { get; init; }
        public int AvailableBeds { get; init; }
        public decimal Price { get; init; }
        public string RoomType { get; init; } = string.Empty;
    }
}
