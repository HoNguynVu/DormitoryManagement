namespace BusinessObject.DTOs.BuildingManagerDTOs
{
    public class BuildingManagerDto
    {
        public string ManagerID { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string? Address { get; init; }
        public IEnumerable<BuildingDto> Buildings { get; init; } = Array.Empty<BuildingDto>();
    }
}
