namespace BusinessObject.DTOs.BuildingManagerDTOs
{
    public class BuildingManagerDto
    {
        public string ManagerID { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string CitizenId { get; init; } = string.Empty;
        public DateTime DateOfBirth { get; init; }
        public string PhoneNumber { get; init; } = string.Empty;
        public string? Address { get; init; }
        public BuildingDto BuildingDto { get; init; } = new BuildingDto();
    }
}
