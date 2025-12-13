namespace BusinessObject.DTOs.RoomDTOs
{
    public record RoomFilterDto
    {
        public string? BuildingId { get; init; }
        public string? RoomTypeId { get; init; }
        public int? MinCapacity { get; init; }
        public int? MaxCapacity { get; init; }
        public bool? OnlyAvailable { get; init; } = true;
    }
}
